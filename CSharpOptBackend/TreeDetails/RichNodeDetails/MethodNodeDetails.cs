using System.Collections.Generic;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeDetails.RichNodeDetails
{
    public class MethodNodeDetails : INodeDetails
    {
        public SyntaxNode Node { get; }
        public Span NodeSpan { get; }
        public IList<Span> HighlightSpans { get; }

        public MethodNodeDetails(SyntaxNode node, Span nodeSpan, Span definitionHighlight, Span entryHighlight, Span exitHighlight)
        {
            Node = node;
            NodeSpan = nodeSpan;
            HighlightSpans = new List<Span> {definitionHighlight, entryHighlight, exitHighlight};
        }
    }
}