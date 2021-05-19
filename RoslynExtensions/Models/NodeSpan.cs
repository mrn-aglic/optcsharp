using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.interfaces;

namespace RoslynExtensions.Models
{
    public class Span
    {
        public int Begin { get; }
        public int End { get; }

        public Span(int begin, int end, bool zeroIndex = true)
        {
            Begin = zeroIndex ? begin + 1 : begin;
            End = zeroIndex ? end + 1 : end;
        }

        public override string ToString()
        {
            return $"({Begin}, {End})";
        }
    }

    public class NodeSpan : INodeSpan
    {
        public Span FirstLine { get; }
        public Span LastLine { get; }

        public NodeSpan(Span first, Span last)
        {
            FirstLine = first;
            LastLine = last;
        }

        public static NodeSpan From(BlockSyntax blockSyntax, bool isEntry = true)
        {
            var lineSpan = blockSyntax.GetLocation().GetLineSpan();
            var start = new Span(lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character + blockSyntax.OpenBraceToken.FullSpan.Length);
            var end = new Span(lineSpan.EndLinePosition.Line,
                lineSpan.EndLinePosition.Character + blockSyntax.CloseBraceToken.FullSpan.Length);

            return new NodeSpan(start, end);
        }

        public static NodeSpan From(SyntaxNode syntaxNode)
        {
            var lineSpan = syntaxNode.GetLocation().GetLineSpan();
            var start = new Span(lineSpan.StartLinePosition.Line, lineSpan.StartLinePosition.Character);
            var end = new Span(lineSpan.EndLinePosition.Line, lineSpan.EndLinePosition.Character);

            return new NodeSpan(start, end);
        }

        public Span GetSingleLineHighlight()
        {
            return new Span(FirstLine.Begin, FirstLine.End, false);
        }

        public Span GetMultilineHighlight()
        {
            return new Span(FirstLine.Begin, LastLine.End, false);
        }

        public override string ToString()
        {
            return $"{FirstLine} - {LastLine}";
        }
    }
}