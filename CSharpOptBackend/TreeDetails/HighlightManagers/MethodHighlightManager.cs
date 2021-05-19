using System;
using System.Linq;
using CSharpOptBackend.TreeDetails.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpOptBackend.TreeDetails.HighlightManagers
{
    public class MethodHighlightManager : IHighlightManager
    {
        public static Func<FileLinePositionSpan, Span> ENTRY = span =>
            new Span(span.StartLinePosition.Line + 1,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                span.StartLinePosition.Character + 1
            );

        public static Func<FileLinePositionSpan, Span> EXIT = span =>
            new Span(span.StartLinePosition.Line + 1,
                span.StartLinePosition.Line + 1,
                span.StartLinePosition.Character + 1,
                span.StartLinePosition.Character + 1
            );

        public Span GetHighlightSpan(SyntaxNode node)
        {
            return GetDefinitionHighlight(node);
        }

        public SyntaxKind GetSyntaxKind()
        {
            return SyntaxKind.MethodDeclaration;
        }

        public Span GetDefinitionHighlight(SyntaxNode node)
        {
            var childNodesAndTokens = node.ChildNodesAndTokens();
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

        public Span GetEntryHighlight(SyntaxNode node)
        {
            var methodDeclaration = (MethodDeclarationSyntax) node;
            return GetEntryHighlight(methodDeclaration);
        }

        private Span GetEntryHighlight(MethodDeclarationSyntax method)
        {
            var anyStatements = method.Body.Statements.Any();

            var node = anyStatements ? method.Body.Statements.First() : method.ChildNodesAndTokens().Last();
            var startLocation = node.GetLocation().GetLineSpan().StartLinePosition;

            return new Span(
                startLocation.Line + 1,
                startLocation.Line + 1,
                startLocation.Character + 1,
                startLocation.Character + 1
            );
        }
        
        public Span GetExitHighlight(SyntaxNode node)
        {
            var methodDeclaration = (MethodDeclarationSyntax) node;
            return GetExitHighlight(methodDeclaration);
        }

        private Span GetExitHighlight(MethodDeclarationSyntax method)
        {
            var anyStatements = method.Body.Statements.Any();

            var node = anyStatements ? method.Body.Statements.Last() : method.ChildNodesAndTokens().Last();
            var endLocation = node.GetLocation().GetLineSpan().EndLinePosition;

            return new Span(
                endLocation.Line + 1,
                endLocation.Line + 1,
                endLocation.Character + 1,
                endLocation.Character + 1
            );
        }
    }
}