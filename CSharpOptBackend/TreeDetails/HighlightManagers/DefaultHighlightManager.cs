using System.Linq;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.TreeDetails
{
    public class DefaultHighlightManager : IHighlightManager
    {
        public DefaultHighlightManager()
        {
        }
        
        
        public Span GetHighlightSpan(SyntaxNode syntaxNode)
        {
            var childNodesAndTokens = syntaxNode.ChildNodesAndTokens();
            var tail = childNodesAndTokens.Take(childNodesAndTokens.Count - 1);

            var start = childNodesAndTokens.First();
            var last = tail.LastOrDefault();

            var startLocation = start.GetLocation().GetLineSpan().StartLinePosition;
            var endLocation = last == null
                ? start.GetLocation().GetLineSpan().EndLinePosition
                : last.GetLocation().GetLineSpan().EndLinePosition;

            return new Span
            (
                startLocation.Line + 1,
                endLocation.Line + 1,
                startLocation.Character + 1,
                endLocation.Character + 1
            );
        }
    }
}