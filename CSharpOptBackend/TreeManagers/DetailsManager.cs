using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpOptBackend.TreeDetails.Interfaces;
using CSharpOptBackend.TreeManagers.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeManagers
{
    public class DetailsManager : IDetailsManager
    {
        private readonly INodeDetailsFactory _nodeDetailsFactory;
        public ImmutableDictionary<string, INodeDetails> TreeDetails { get; private set; }

        public DetailsManager(INodeDetailsFactory nodeDetailsFactory)
        {
            _nodeDetailsFactory = nodeDetailsFactory;
            TreeDetails = ImmutableDictionary<string, INodeDetails>.Empty;
        }

        public INodeDetails GetNodeDetails(SyntaxNode node)
        {
            return _nodeDetailsFactory.Create(node);
        }

        public void AddDetails(INodeDetails details)
        {
            var self = (IDetailsManager) this;
            TreeDetails = TreeDetails.Add(self.GetNodeKey(details.Node), details);
        }

        public IEnumerable<INodeDetails> GetDetails()
        {
            return TreeDetails.Select(x => x.Value);
        }
    }
}