using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Models;

namespace RoslynExtensions.Helpers
{
    internal static class SpanHelper
    {
        public static NodeSpan GetSpan(SyntaxNode node)
        {
            switch (node)
            {
                case BlockSyntax blockSyntax:
                    return NodeSpan.From(blockSyntax);
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return NodeSpan.From(methodDeclarationSyntax);
                default:
                    return NodeSpan.From(node);
            }
        }
    }
}