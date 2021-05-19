using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynExtensions.Extensions
{
    public static class ExpressionSyntaxExtensions
    {
        public static ArgumentSyntax ToNamedArgument(this ExpressionSyntax node, string name)
        {
            return string.IsNullOrWhiteSpace(name)
                ? SyntaxFactory.Argument(node)
                : SyntaxFactory.Argument(node).WithNameColon(SyntaxFactory.NameColon(name));
        }
    }
}