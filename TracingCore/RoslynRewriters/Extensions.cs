using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using TracingCore.Common;

namespace TracingCore.RoslynRewriters
{
    public static class Extensions
    {
        public static bool IsBlock(this SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.Block);
        }

        public static bool IsStatic<T>(this T node) where T : MemberDeclarationSyntax
        {
            return node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
        }

        public static FileLinePositionSpan GetLineSpan(this SyntaxNode node)
        {
            return node.GetLocation().GetLineSpan();
        }

        public static LineData GetLineData(this SyntaxNode node, bool useEndLine)
        {
            var span = node.GetLineSpan();
            return new LineData(useEndLine ? span.EndLinePosition.Line + 1 : span.StartLinePosition.Line + 1);
        }

        public static ILineData GetFullLineData(this SyntaxNode node, bool useEndLine)
        {
            var span = node.GetLineSpan();
            return new TraceLineSpan(span, useEndLine);
        }

        public static T GetParentOfType<T>(this SyntaxNode node)
        {
            return node.Ancestors().OfType<T>().FirstOrDefault();
        }
    }
}