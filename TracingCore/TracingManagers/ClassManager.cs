using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TraceToPyDtos;

namespace TracingCore.TracingManagers
{
    public class ClassManager
    {
        private readonly string _mainMethod = "Main";
        private readonly Dictionary<string, ClassData> _classes;
        private readonly SemanticModel _semanticModel;
        
        private readonly IList<SyntaxKind> _supportedMemberKinds = new List<SyntaxKind>
        {
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration
        };

        public ClassManager(Dictionary<string, ClassData> classes, SemanticModel semanticModel)
        {
            _classes = classes;
            _semanticModel = semanticModel;
        }

        private string GetParameterTypes(ParameterListSyntax parameterListSyntax)
        {
            return string.Join(",", parameterListSyntax.Parameters.Select(x => x.Identifier.Text));
        }

        private MethodData GetMemberData
        (
            BaseMethodDeclarationSyntax declarationSyntax,
            SyntaxToken identifier,
            TypeSyntax returnType
        )
        {
            var paramTypes = GetParameterTypes(declarationSyntax.ParameterList);
            var isMain = identifier.Text == _mainMethod &&
                         declarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
            return new MethodData(
                identifier.Text,
                RoslynHelper.GetLineData(declarationSyntax).StartLine,
                $"{returnType} {identifier.Text}({paramTypes})".TrimStart(),
                isMain
            );
        }

        private MethodData GetMemberData(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var identifier = methodDeclarationSyntax.Identifier;
            var returnType = methodDeclarationSyntax.ReturnType;
            return GetMemberData(methodDeclarationSyntax, identifier, returnType);
        }

        private MethodData GetMemberData(ConstructorDeclarationSyntax constructorDeclarationSyntax)
        {
            var identifier = constructorDeclarationSyntax.Identifier;
            return GetMemberData(constructorDeclarationSyntax, identifier, null);
        }
        
        private MethodData GetMemberData(BaseMethodDeclarationSyntax declarationSyntax)
        {
            switch (declarationSyntax)
            {
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return GetMemberData(methodDeclarationSyntax);
                case ConstructorDeclarationSyntax constructorDeclarationSyntax:
                    return GetMemberData(constructorDeclarationSyntax);
                default:
                    return null;
            }
        }

        private bool IsSupported(BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
        {
            return _supportedMemberKinds.Contains(baseMethodDeclarationSyntax.Kind());
        }

        public void RegisterClasses(CompilationUnitSyntax root)
        {
            var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            var classesData = namespaces.SelectMany(x =>
                x.ChildNodes().OfType<ClassDeclarationSyntax>()
                    .Select(y =>
                        new ClassData
                        (
                            y.Identifier.Text.ToString(),
                            string.Empty, // IMPLEMENT
                            RoslynHelper.GetClassParentPath(y),
                            y.DescendantNodes().OfType<BaseMethodDeclarationSyntax>()
                                .Where(IsSupported)
                                .Select(GetMemberData)
                                .ToList(),
                            RoslynHelper.GetLineData(y)
                        )
                    )
            );

            foreach (var classData in classesData)
            {
                _classes.Add(classData.FullyQualifiedPath, classData);
            }
        }

        public ClassData GetClassData(string fullyQualifiedName)
        {
            return _classes.GetValueOrDefault(fullyQualifiedName);
        }
    }
}