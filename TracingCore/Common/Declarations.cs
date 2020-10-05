using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.RoslynRewriters;

namespace TracingCore.Common
{
    public static class Declarations
    {
        private static readonly ImmutableHashSet<SyntaxKind> MethodLikeDeclarations =
            ImmutableHashSet<SyntaxKind>.Empty.Union
            (
                new List<SyntaxKind>
                {
                    SyntaxKind.GetAccessorDeclaration,
                    SyntaxKind.SetAccessorDeclaration,
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.ConstructorDeclaration
                }
            );

        private static bool IsMethodLike(SyntaxKind kind)
        {
            return MethodLikeDeclarations.Contains(kind);
        }

        public static bool IsMethodLike(SyntaxNode node)
        {
            return IsMethodLike(node.Kind());
        }

        public static bool IsInStaticContext(SyntaxNode node)
        {
            return node.GetParentOfType<MemberDeclarationSyntax>().IsStatic() ||
                   node.GetParentOfType<ClassDeclarationSyntax>().IsStatic();
        }

        public static bool NonStaticContext(SyntaxNode node)
        {
            return !IsInStaticContext(node);
        }
    }
}