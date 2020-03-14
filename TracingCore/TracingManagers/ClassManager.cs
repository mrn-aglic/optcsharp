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
    public class ClassManager
    {
        private readonly string _mainMethod = "Main";
        private readonly Dictionary<string, ClassData> _classes;
        private readonly SemanticModel _semanticModel;
        private readonly Assembly _assembly;
        private readonly AnnotationsManager _annotationsManager;

        public ClassManager(CompilationResult compilationResult,
            Dictionary<string, ClassData> classes)
        {
            _classes = classes;
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
            var isMain = identifier.Text == _mainMethod &&
                         declarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
            return new MethodData(
                identifier.Text,
                RoslynHelper.GetLineData(declarationSyntax).StartLine,
                $"{returnType} {identifier.Text}({paramTypes})".TrimStart(),
                isMain
            );
        }

        private IList<MethodData> GetMemberDataForProperty(PropertyDeclarationSyntax property)
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

        private string FullyQualifiedName(BaseTypeSyntax typeSyntax)
        {
            return _semanticModel.GetSymbolInfo(typeSyntax.Type).Symbol.ToString();
        }

        public string[] GetExtendedTypes(ClassDeclarationSyntax declarationSyntax)
        {
            var baseList = declarationSyntax.BaseList;
            return baseList == null ? new string[] { } : baseList.Types.Select(FullyQualifiedName).ToArray();
        }

        public IList<MethodData> GetMethodsAndProperties(ClassDeclarationSyntax @class)
        {
            var members = @class.Members.Where(_annotationsManager.IsNotAugmentation).ToList();
            var baseMethodTypes = members.OfType<BaseMethodDeclarationSyntax>();
            var properties = members.OfType<PropertyDeclarationSyntax>();

            var accessors = properties.SelectMany(GetMemberDataForProperty);
            var methods = baseMethodTypes.Select(GetMemberData);

            return accessors.Concat(methods).ToList();
        }

        private ClassData ConstructClassData(ClassDeclarationSyntax @class)
        {
            var fullName = RoslynHelper.GetClassParentPath(@class);
            return new ClassData
            (
                @class.Identifier.Text,
                @class.Modifiers.Any(SyntaxKind.StaticKeyword),
                GetExtendedTypes(@class),
                fullName,
                GetMethodsAndProperties(@class),
                _annotationsManager
                    .GetLineDataFromAnnotation(@class.GetAnnotations(_annotationsManager.OriginalLineAnnotation.Kind)
                        .FirstOrDefault()).startLine,
                _assembly.GetType(fullName)
            );
        }

        public void RegisterClasses()
        {
            var root = _semanticModel.SyntaxTree.GetCompilationUnitRoot();
            var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
            var classes = namespaces.Any()
                ? namespaces.SelectMany(x => x.ChildNodes().OfType<ClassDeclarationSyntax>()).ToList()
                : root.ChildNodes().OfType<ClassDeclarationSyntax>();

            var classesData = classes.Select(ConstructClassData);

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