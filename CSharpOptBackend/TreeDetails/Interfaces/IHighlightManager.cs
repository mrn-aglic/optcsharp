using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpOptBackend.TreeDetails.Interfaces
{
    public interface IHighlightManager
    {
        public Span GetHighlightSpan(SyntaxNode node);
        public SyntaxKind GetSyntaxKind();
    }
}