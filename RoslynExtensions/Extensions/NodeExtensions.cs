using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RoslynExtensions.Helpers;
using RoslynExtensions.Models;

namespace RoslynExtensions.Extensions
{
    public static class NodeExtensions
    {
        public static T GetParentOfType<T>(this SyntaxNode node)
        {
            return node.Ancestors().OfType<T>().FirstOrDefault();
        }

        public static NodeSpan GetSpan(this SyntaxNode node)
        {
            return SpanHelper.GetSpan(node);
        }

        public static bool IsMember(this SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.PropertyDeclaration) ||
                   node.IsKind(SyntaxKind.MethodDeclaration) ||
                   node.IsKind(SyntaxKind.ConstructorDeclaration);
        }

        public static bool IsMemberOrClass(this SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.ClassDeclaration) || node.IsMember();
        }
    }
}