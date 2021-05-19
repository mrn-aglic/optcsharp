using System.Collections.Generic;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.Visitors
{
    public interface IDetailsManager
    {
        public Dictionary<string, INodeDetails> TreeDetails { get; }

        public string GetNodeKey(SyntaxNode node)
        {
            return $"{node.GetLocation()}-{node.SyntaxTree.FilePath}";
        }
    }
}