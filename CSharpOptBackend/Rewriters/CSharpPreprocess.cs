using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpOptBackend.Rewriters
{
    public class CSharpPreprocess : CSharpSyntaxRewriter
    {
        private const string ReturnVarPrefix = "__RETURN__";

        public CompilationUnitSyntax Rewriter(CSharpSyntaxTree tree)
        {
            var compilationUnit = tree.GetCompilationUnitRoot();
            base.Visit(compilationUnit);
            return compilationUnit;
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Statement.IsKind(SyntaxKind.Block) && node?.Else?.Statement is BlockSyntax) return node;

            var newNode = node.Statement.IsKind(SyntaxKind.Block)
                ? node
                : node.WithStatement(SyntaxFactory.Block(node.Statement)).WithElse(node.Else);

            if (newNode.Else == null) return newNode;

            newNode = newNode.Else != null && newNode?.Else.Statement is BlockSyntax
                ? newNode
                : newNode.WithElse(newNode.Else.WithStatement(SyntaxFactory.Block(newNode.Else.Statement)));

            return newNode;
        }

        public override SyntaxNode? VisitForStatement(ForStatementSyntax node)
        {
            if (node.Statement.IsKind(SyntaxKind.Block)) return node;

            return node.WithStatement(SyntaxFactory.Block(node.Statement));
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            if (node.Statement.IsKind(SyntaxKind.Block)) return node;

            return node.WithStatement(SyntaxFactory.Block(node.Statement));
        }

        public override SyntaxNode? VisitBlock(BlockSyntax node)
        {
            var returns = node.DescendantNodes().Where(d => d.IsKind(SyntaxKind.ReturnStatement));
            if (!returns.Any()) return node;


            // return node.WithStatements()

            throw new NotImplementedException("TODO: REPLACE RETURN STATEMENTS");
        }

        private IEnumerable<StatementSyntax> ProcessBlockStatements(IEnumerable<StatementSyntax> statements)
        {
            return statements.SelectMany(st => st is ReturnStatementSyntax rs
                ? ReplaceReturn(rs)
                : new List<StatementSyntax> {st});
        }

        private IEnumerable<StatementSyntax> ReplaceReturn(ReturnStatementSyntax returnStatement)
        {
            var lineSpan = returnStatement.FullSpan;
            var varName = $"{ReturnVarPrefix}_{lineSpan}";
            
            var declaration = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(Identifier(
                        TriviaList(),
                        SyntaxKind.VarKeyword,
                        "var",
                        "var",
                        TriviaList()
                    )))
                    .WithVariables(
                        new SeparatedSyntaxList<VariableDeclaratorSyntax>
                        {
                            VariableDeclarator(Identifier(varName))
                                .WithInitializer(EqualsValueClause(returnStatement.Expression ??
                                                                   throw new InvalidOperationException(
                                                                       "Return statement must have expression")))
                        })
            );
            var newReturnStatement = ReturnStatement(IdentifierName(varName));
            return new List<StatementSyntax>
            {
                declaration,
                newReturnStatement
            };
        }
    }
}