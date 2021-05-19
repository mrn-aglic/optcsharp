using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpOptBackend.DebuggerCommands
{
    public class ScopeManager
    {
        public IList<ScopedVar> GetScopedVars(MethodDeclarationSyntax methodDeclaration)
        {
            var parameters = methodDeclaration.ParameterList.Parameters;
            var existingVars = parameters.Select(
                p => new ScopedVar(methodDeclaration, p.Identifier.Text, methodDeclaration.Body));
            return existingVars.ToList();
        }

        public IList<ScopedVar> GetScopedVars(BlockSyntax blockSyntax, IList<ScopedVar> existingVars)
        {
            var newScope = new List<ScopedVar>(existingVars);

            foreach (var statement in blockSyntax.Statements)
            {
                var scoped = CreateScopedVars(statement, newScope);
                newScope.AddRange(scoped);
            }

            return newScope;
        }

        public IList<ScopedVar> CreateScopedVars(SyntaxNode node)
        {
            switch (node)
            {
                case ForStatementSyntax forStatement:
                    return CreateScopedVars(forStatement);
                case VariableDeclarationSyntax variableDeclaration:
                    return CreateScopedVars(variableDeclaration);
                case LocalDeclarationStatementSyntax localDeclaration:
                    return CreateScopedVars(localDeclaration);
                case StatementSyntax statementSyntax:
                    return new List<ScopedVar>();
                default:
                    throw new NotImplementedException($"{node.Kind()}");
            }
        }

        public IList<ScopedVar> CreateScopedVars(ForStatementSyntax forStatement)
        {
            if (forStatement.Declaration == null) return new List<ScopedVar>();
            var vars = forStatement.Declaration.Variables;
            var scopedVars = vars.Select(v =>
                    new ScopedVar(forStatement, v.Identifier.Text, forStatement))
                .ToList();

            return scopedVars;
        }

        public IList<ScopedVar> CreateScopedVars(LocalDeclarationStatementSyntax localDeclaration)
        {
            return CreateScopedVars(localDeclaration.Declaration);
        }

        public IList<ScopedVar> CreateScopedVars(VariableDeclarationSyntax variableDeclaration)
        {
            var parent = GetParentScope(variableDeclaration);
            var vars = variableDeclaration.Variables;
            var newScoped = vars.Select(v =>
                    new ScopedVar(parent, v.Identifier.Text, variableDeclaration))
                .ToList();
            return newScoped;
        }

        private IList<ScopedVar> CreateScopedVars(StatementSyntax statementSyntax, IList<ScopedVar> existingScopedVars)
        {
            switch (statementSyntax)
            {
                case LocalDeclarationStatementSyntax localDeclarationStatement:
                    return CreateScopedVars(localDeclarationStatement, existingScopedVars);
                case BlockSyntax blockSyntax:
                    return CreateScopedVars(blockSyntax, existingScopedVars);
                default:
                    throw new NotImplementedException();
            }
        }

        private SyntaxNode GetParentScope(SyntaxNode node)
        {
            return (node.Parent is LocalDeclarationStatementSyntax ? node.Parent : node)
                .Ancestors().First(n => n is StatementSyntax);
        }
    }
}