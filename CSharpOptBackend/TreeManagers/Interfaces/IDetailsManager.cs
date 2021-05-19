using System.Collections.Generic;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeManagers.Interfaces
{
    public interface IDetailsManager
    {
        public string GetNodeKey(SyntaxNode node)
        {
            return $"{node.GetLocation()}-{node.SyntaxTree.FilePath}";
        }

        public INodeDetails GetNodeDetails(SyntaxNode node);

        public void AddDetails(INodeDetails details);

        public IEnumerable<INodeDetails> GetDetails();
    }
}