using System;
using System.Collections.Generic;
using System.Linq;
using CSharpOptBackend.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;

namespace CSharpOptBackend.DebuggerCommands
{
    public class DebugStatementManager : IDebugStatementManager
    {
        private bool _includeMainArgs = false;
        public ScopeManager ScopeManager { get; }
        public IList<DebugStatement> DebugStatements { get; }

        public DebugStatementManager(ScopeManager scopeManager)
        {
            ScopeManager = scopeManager;
            DebugStatements = new List<DebugStatement>();
        }

        public IList<DebugStatement> CreateDebugStatements(CompilationUnitSyntax compilationUnit)
        {
            var descendants = compilationUnit.DescendantNodes();
            var members = descendants.Where(desc => desc.IsMember());

            var debugStatements = new List<DebugStatement>();

            foreach (var member in members)
            {
                var newDebugStatements = CreateDebugStatementsForMember(member);
                debugStatements.AddRange(newDebugStatements);
            }

            return debugStatements;
        }

        private IList<DebugStatement> CreateDebugStatementsForMember(SyntaxNode node)
        {
            switch (node)
            {
                case MethodDeclarationSyntax methodDeclaration:
                    return CreateForMethod(methodDeclaration);
                default:
                    throw new NotImplementedException($"Can't get statement for {node.Kind()}");
            }
        }

        private IList<DebugStatement> CreateForMethod(MethodDeclarationSyntax methodDeclaration)
        {
            var isMain = methodDeclaration.IsStatic() && methodDeclaration.Identifier.Text == "Main";
            var scopedVars = ScopeManager.GetScopedVars(methodDeclaration).Where(x => !isMain).ToList();
            var bodyStatements = Create(methodDeclaration.Body, scopedVars);
            return bodyStatements;
        }

        private bool ShouldAccept(SyntaxNode node)
        {
            return node is StatementSyntax;
        }

        private bool CreatesScope(SyntaxNode node)
        {
            return node.IsKind(SyntaxKind.Block) ||
                   node.IsKind(SyntaxKind.ForStatement);
        }

        private IList<DebugStatement> Create(SyntaxNode node, IList<ScopedVar> existingScopedVars)
        {
            (IList<DebugStatement> debugStatements, IList<ScopedVar> scopedVars) CreateInner(
                SyntaxNode _node,
                List<ScopedVar> currentScope)
            {
                var debugStatements = new List<DebugStatement>();

                var myScope = new List<ScopedVar>(currentScope);

                if (!(_node is BlockSyntax))
                {
                    var vars = ScopeManager.CreateScopedVars(_node);
                    myScope.AddRange(vars);

                    var debugStatement = new DebugStatement(_node, myScope.Select(v => v.Name));
                    debugStatements.Add(debugStatement);
                }

                foreach (var childNode in _node.ChildNodes().Where(ShouldAccept))
                {
                    var (statements, vars) = CreateInner(childNode, myScope);
                    debugStatements.AddRange(statements);

                    if (!CreatesScope(childNode))
                        myScope.AddRange(vars);
                }

                var newVars = myScope.Where(s => !currentScope.Contains(s)).ToList();

                return (debugStatements, newVars);
            }

            return CreateInner(node, existingScopedVars.ToList()).debugStatements;
        }
    }
}