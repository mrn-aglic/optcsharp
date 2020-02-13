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

        public ClassManager(SemanticModel semanticModel, Dictionary<string, ClassData> classes)
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

        private string FullyQualifiedName(BaseTypeSyntax typeSyntax)
        {
            return _semanticModel.GetSymbolInfo(typeSyntax.Type).Symbol.ToString();
        }

        public string[] GetExtendedTypes(ClassDeclarationSyntax declarationSyntax)
        {
            var baseList = declarationSyntax.BaseList;
            return baseList == null ? new string[] { } : baseList.Types.Select(FullyQualifiedName).ToArray();
        }

        public void RegisterClasses(CompilationUnitSyntax root)
        {
            var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
            var classes = namespaces.Any()
                ? namespaces.SelectMany(x => x.ChildNodes().OfType<ClassDeclarationSyntax>()).ToList()
                : root.ChildNodes().OfType<ClassDeclarationSyntax>();

            var classesData = classes
                .Select(@class =>
                    new ClassData
                    (
                        @class.Identifier.Text.ToString(),
                        GetExtendedTypes(@class), // IMPLEMENT
                        RoslynHelper.GetClassParentPath(@class),
                        @class.DescendantNodes().OfType<BaseMethodDeclarationSyntax>()
                            .Where(IsSupported)
                            .Select(GetMemberData)
                            .ToList(),
                        RoslynHelper.GetLineData(@class)
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