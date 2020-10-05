using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TraceToPyDtos;

namespace TracingCore.TracingManagers
{
    public class StructManager
    {
        private readonly Dictionary<string, StructData> _structs;
        private readonly SemanticModel _semanticModel;

        private readonly ManagerCore _managerCore;

        public StructManager(CompilationResult compilationResult,
            Dictionary<string, StructData> structs)
        {
            _structs = structs;
            _semanticModel = compilationResult.GetSemanticModel();
            _managerCore = new ManagerCore(compilationResult);
        }

        public void RegisterStructs()
        {
            var root = _semanticModel.SyntaxTree.GetCompilationUnitRoot();
            var structs = root.DescendantNodes().OfType<StructDeclarationSyntax>();
            var structsData = structs.Select(_managerCore.ConstructStructData);

            foreach (var data in structsData)
            {
                _structs.Add(data.FullyQualifiedPath, data);
            }
        }

        public IEnumerable<StructData> GetStructData()
        {
            return _structs.Values.ToList();
        }
    }
}