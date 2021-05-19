using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeDetails.Interfaces
{
    public interface INodeDetails
    {
        public SyntaxNode Node { get; }
        public Span NodeSpan { get; }
        public IList<Span> HighlightSpans { get; }
    }
}