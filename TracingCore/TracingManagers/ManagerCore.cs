using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.SyntaxTreeEnhancers;
using TracingCore.TraceToPyDtos;

namespace TracingCore.TracingManagers
{
    public class ManagerCore
    {
        private const string MainMethod = "Main";
        private readonly SemanticModel _semanticModel;
        private readonly Assembly _assembly;
        private readonly AnnotationsManager _annotationsManager;

        public ManagerCore(CompilationResult compilationResult)
        {
            _assembly = compilationResult.Assembly;
            _semanticModel = compilationResult.GetSemanticModel();
            _annotationsManager = new AnnotationsManager();
        }

        private string GetParameterTypes(ParameterListSyntax parameterListSyntax)
        {
            return string.Join(",", parameterListSyntax.Parameters.Select(x => x.Type));
        }

        private MethodData GetMemberData
        (
            BaseMethodDeclarationSyntax declarationSyntax,
            SyntaxToken identifier,
            TypeSyntax returnType
        )
        {
            var paramTypes = GetParameterTypes(declarationSyntax.ParameterList);
            var isMain = identifier.Text == MainMethod &&
                         declarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
            return new MethodData(
                identifier.Text,
                RoslynHelper.GetLineData(declarationSyntax).StartLine,
                $"{returnType} {identifier.Text}({paramTypes})".TrimStart(),
                isMain
            );
        }

        private IList<MethodData> GetMemberData(PropertyDeclarationSyntax property)
        {
            var propertyName = property.Identifier.Text;
            var accessors = property.AccessorList.Accessors;
            var propertyType = property.Type.ToString();

            return accessors
                .Select(x =>
                    new MethodData(
                        propertyName,
                        RoslynHelper.GetLineData(x).StartLine,
                        $"{propertyType} {propertyName} {x.Keyword.ToString()}",
                        false
                    )).ToList();
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
            return declarationSyntax switch
            {
                MethodDeclarationSyntax methodDeclarationSyntax => GetMemberData(methodDeclarationSyntax),
                ConstructorDeclarationSyntax constructorDeclarationSyntax =>
                GetMemberData(constructorDeclarationSyntax),
                _ => null
            };
        }

        private string FullyQualifiedName(BaseTypeSyntax typeSyntax)
        {
            return ModelExtensions.GetSymbolInfo(_semanticModel, typeSyntax.Type).Symbol.ToString();
        }

        private string[] GetExtendedTypes(TypeDeclarationSyntax declarationSyntax)
        {
            var baseList = declarationSyntax.BaseList;
            return baseList == null ? new string[] { } : baseList.Types.Select(FullyQualifiedName).ToArray();
        }

        private IList<MethodData> GetMethodsAndProperties(TypeDeclarationSyntax typeDeclaration)
        {
            var members = typeDeclaration.Members.Where(_annotationsManager.IsNotAugmentation).ToList();
            var baseMethodTypes = members.OfType<BaseMethodDeclarationSyntax>();
            var properties = members.OfType<PropertyDeclarationSyntax>();

            var accessors = properties.SelectMany(GetMemberData);
            var methods = baseMethodTypes.Select(GetMemberData);

            return accessors.Concat(methods).ToList();
        }

        public ClassData ConstructClassData(TypeDeclarationSyntax typeDeclaration)
        {
            var fullName = RoslynHelper.GetClassParentPath(typeDeclaration);
            return new ClassData
            (
                typeDeclaration.Identifier.Text,
                typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                GetExtendedTypes(typeDeclaration),
                fullName,
                GetMethodsAndProperties(typeDeclaration),
                _annotationsManager
                    .GetLineDataFromAnnotation(typeDeclaration
                        .GetAnnotations(_annotationsManager.OriginalLineAnnotation.Kind)
                        .FirstOrDefault()).startLine,
                _assembly.GetType(fullName)
            );
        }
        
        public StructData ConstructStructData(TypeDeclarationSyntax typeDeclaration)
        {
            var fullName = RoslynHelper.GetClassParentPath(typeDeclaration);
            return new StructData
            (
                typeDeclaration.Identifier.Text,
                typeDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword),
                GetExtendedTypes(typeDeclaration),
                fullName,
                GetMethodsAndProperties(typeDeclaration),
                _annotationsManager
                    .GetLineDataFromAnnotation(typeDeclaration
                        .GetAnnotations(_annotationsManager.OriginalLineAnnotation.Kind)
                        .FirstOrDefault()).startLine,
                _assembly.GetType(fullName)
            );
        }
    }
}