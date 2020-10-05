using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TreeRewriters;

namespace TracingCore.Common
{
    public static class RoslynHelper
    {
        public static string GetClassParentPath(TypeDeclarationSyntax classDeclarationSyntax)
        {
            bool ClassOrNamespaceKind(SyntaxNode node)
            {
                return node.IsKind(SyntaxKind.NamespaceDeclaration) ||
                       node.IsKind(SyntaxKind.ClassDeclaration);
            }

            var names = classDeclarationSyntax
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

        public static string ClassNameFromFullName(string fullyQualifiedName)
        {
            return fullyQualifiedName.Substring(fullyQualifiedName.LastIndexOf(".", StringComparison.Ordinal));
        }

        public static LineData GetLineData
        (
            SyntaxNode syntaxNode,
            bool useEndLine = false
        )
        {
            var lineSpan = syntaxNode.GetLocation().GetLineSpan();
            return new LineData
            (
                useEndLine ? lineSpan.EndLinePosition.Line + 1 : lineSpan.StartLinePosition.Line + 1
            );
        }
    }
}