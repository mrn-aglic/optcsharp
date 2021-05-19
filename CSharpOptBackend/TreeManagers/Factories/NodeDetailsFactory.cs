using System.Collections.Generic;
using System.Linq;
using CSharpOptBackend.TreeDetails;
using CSharpOptBackend.TreeDetails.HighlightManagers;
using CSharpOptBackend.TreeDetails.Interfaces;
using CSharpOptBackend.TreeManagers.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpOptBackend.TreeManagers.Factories
{
    public class NodeDetailsFactory : INodeDetailsFactory
    {
        private readonly IHighlightManager _highlightManager;
        private readonly Dictionary<SyntaxKind, IHighlightManager> _highlightManagers;

        public NodeDetailsFactory(Dictionary<SyntaxKind, IHighlightManager> highlightManagers)
        {
            _highlightManagers = highlightManagers.Any()
                ? highlightManagers
                : new Dictionary<SyntaxKind, IHighlightManager>
                    {{SyntaxKind.None, new DefaultHighlightManager()}};
        }

        public NodeDetailsFactory(IHighlightManager highlightManager)
        {
            _highlightManager = highlightManager;
        }

        private INodeDetails CreateMethodNodeDetails(SyntaxNode node)
        {
            var highlightManager = (MethodHighlightManager) _highlightManagers[SyntaxKind.MethodDeclaration];
            var def = highlightManager.GetDefinitionHighlight(node);
            var entry = highlightManager.GetEntryHighlight(node);
            var exit = highlightManager.GetExitHighlight(node);
            return new NodeDetails(node, new List<Span> {def, entry, exit});
        }

        public INodeDetails Create(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case MethodDeclarationSyntax:
                    return CreateMethodNodeDetails(syntaxNode);
                default:
                    return new NodeDetails(syntaxNode, new List<Span> {_highlightManager.GetHighlightSpan(syntaxNode)});
            }
        }
    }
}