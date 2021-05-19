using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynExtensions.Extensions
{
    public static class MemberExtensions
    {
        public static bool IsStatic<T>(this T node) where T : MemberDeclarationSyntax
        {
            return node.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));
        }
    }
}