using System;
using System.Collections.Generic;
using CSharpOptBackend.DebuggerCommands;
using CSharpOptBackend.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;

namespace CSharpOptBackend.Visitors
{
    public class CSharpWalker : CSharpSyntaxWalker
    {
        private readonly IDebugStatementManager _statementsManager;

        public CSharpWalker(IDebugStatementManager statementsManager)
        {
            _statementsManager = statementsManager;
        }

        public override void VisitBlock(BlockSyntax node)
        {
            base.VisitBlock(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var cmd = new DebugStatement(node, new List<string>());
            Console.WriteLine(node.GetText());
            Console.WriteLine(node.GetSpan());
            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var cmd = new DebugStatement(node, new List<string>());
            Console.WriteLine(node.GetText());
            Console.WriteLine(node.GetSpan());
            base.VisitInvocationExpression(node);
        }

        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
        }
    }
}