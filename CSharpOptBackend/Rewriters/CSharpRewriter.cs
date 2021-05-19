using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxComposer;

namespace CSharpOptBackend.Rewriters
{
    public class CSharpRewriter : CSharpSyntaxRewriter
    {
        private readonly ISyntaxComposer _syntaxComposer;

        public CSharpRewriter(ISyntaxComposer syntaxComposer)
        {
            _syntaxComposer = syntaxComposer;
        }

        public CSharpSyntaxTree Rewrite(CSharpSyntaxTree tree, SemanticModel semanticModel = null)
        {
            var root = tree.GetRoot();
            var traceable = root.TrackNodes(root.DescendantNodesAndSelf());

            var result = Visit(traceable);

            Console.WriteLine(result.NormalizeWhitespace().SyntaxTree.GetText());

            return result.SyntaxTree as CSharpSyntaxTree;
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            foreach (var childNode in node.ChildNodes())
            {
                base.Visit(childNode);
            }


            return _syntaxComposer.MethodWrappers.CreateInvocationWrapper("RegisterMethodInvocation", node);
        }

        public override SyntaxNode? VisitReturnStatement(ReturnStatementSyntax node)
        {
            return _syntaxComposer.MethodWrappers.CreateReturnWrapper("RegisterMethodReturn", node);
        }
    }
}