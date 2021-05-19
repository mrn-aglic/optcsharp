using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SyntaxComposer.Shared.interfaces
{
    public interface IBaseMethods
    {
        public ThisExpressionSyntax GetThis()
        {
            return ThisExpression();
        }

        private string GetFullClassName(SyntaxNode node)
        {
            bool ClassOrNamespaceKind(SyntaxNode ancestor)
            {
                return ancestor.IsKind(SyntaxKind.NamespaceDeclaration) ||
                       ancestor.IsKind(SyntaxKind.ClassDeclaration);
            }

            var names = node
                .AncestorsAndSelf()
                .Where(ClassOrNamespaceKind)
                .Select(x =>
                    x is ClassDeclarationSyntax syntax
                        ? syntax.Identifier.Text
                        : ((NamespaceDeclarationSyntax) x).Name.ToString()
                )
                .Reverse();

            return string.Join(".", names);
        }

        public LiteralExpressionSyntax GetContainingClassFullName(SyntaxNode node)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression,
                Literal(GetFullClassName(node)));
        }

        public ExpressionSyntax GetContext(SyntaxNode node)
        {
            var methodDeclaration = node.GetParentOfType<MethodDeclarationSyntax>();
            if (methodDeclaration == null) throw new Exception("Tried to get context outside of Method declaration");

            var isStatic = methodDeclaration.IsStatic();
            return isStatic ?  GetContainingClassFullName(node) : GetThis();
        }
    }
}