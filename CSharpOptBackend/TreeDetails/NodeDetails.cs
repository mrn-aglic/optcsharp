using System.Collections.Generic;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeDetails
{
    public class NodeDetails : INodeDetails
    { 
        public SyntaxNode Node { get; }
        public Span NodeSpan { get; }
        public IList<Span> HighlightSpans { get; }

        public NodeDetails(SyntaxNode node, IList<Span> highlightSpans)
        {
            Node = node;
            NodeSpan = new Span(node.GetLocation());
            HighlightSpans = highlightSpans;
        }
    }
}