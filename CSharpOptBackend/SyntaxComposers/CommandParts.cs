using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpOptBackend.SyntaxComposers
{
    public static class CommandParts
    {
        public static ExpressionSyntax GetHighlightSpan(Span span)
        {
            return TupleExpression
            (
                SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                {
                    Argument(Expression.GetLiteralExpression(span.Begin)),
                    Argument(Expression.GetLiteralExpression(span.End))
                })
            );
        }
    }
}