using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeManagers.Factories
{
    public interface INodeDetailsFactory
    {
        public INodeDetails Create(SyntaxNode node);
    }
}