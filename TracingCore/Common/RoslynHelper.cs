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
        public static string GetClassParentPath(ClassDeclarationSyntax classDeclarationSyntax)
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

        public static ClassDeclarationSyntax FindFirstClassParent(SyntaxNode node)
        {
            return node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }

        public static bool NodeHasStaticModifier(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax classDeclarationSyntax =>
                classDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)),
                AccessorDeclarationSyntax accessorDeclarationSyntax => accessorDeclarationSyntax.Modifiers.Any(x =>
                    x.IsKind(SyntaxKind.StaticKeyword)),
                BaseMethodDeclarationSyntax baseMethodDeclarationSyntax => baseMethodDeclarationSyntax.Modifiers.Any(
                    x => x.IsKind(SyntaxKind.StaticKeyword)),
                _ => throw new NotImplementedException(
                    "The use case probably is not implemented yet or a bug occurred.")
            };
        }

        public static bool IsParentClassStatic(SyntaxNode node)
        {
            var parent = FindFirstClassParent(node);
            return parent != null && NodeHasStaticModifier(parent);
        }
    }
}