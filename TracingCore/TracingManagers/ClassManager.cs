using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TraceToPyDtos;

namespace TracingCore.TracingManagers
{
    public class ClassManager
    {
        private readonly Dictionary<string, ClassData> _classes;
        private readonly SemanticModel _semanticModel;

        private readonly ManagerCore _managerCore;

        public ClassManager(CompilationResult compilationResult,
            Dictionary<string, ClassData> classes)
        {
            _classes = classes;
            _semanticModel = compilationResult.GetSemanticModel();
            _managerCore = new ManagerCore(compilationResult);
        }

        public void RegisterClasses()
        {
            var root = _semanticModel.SyntaxTree.GetCompilationUnitRoot();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
            // var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().ToList();
            // var classes = namespaces.Any()
                // ? namespaces.SelectMany(x => x.ChildNodes().OfType<ClassDeclarationSyntax>()).ToList()
                // : root.ChildNodes().OfType<ClassDeclarationSyntax>();

            var classesData = classes.Select(_managerCore.ConstructClassData);

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