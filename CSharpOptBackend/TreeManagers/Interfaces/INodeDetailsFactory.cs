using System.Collections.Generic;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeManagers.Interfaces
{
    public interface INodeDetailsFactory
    {
        public INodeDetails Create(SyntaxNode node);
    }
}