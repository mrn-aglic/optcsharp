using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxComposer.Shared.interfaces;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SyntaxComposer.Shared
{
    public class LambdaGenerator : ILambdaGenerator
    {
        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ExpressionStatementSyntax expressionStatement)
        {
            return CreateSimpleLambdaExpression(expressionStatement.Expression);
        }

        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ReturnStatementSyntax returnStatementSyntax)
        {
            return CreateSimpleLambdaExpression(returnStatementSyntax.Expression);
        }

        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ExpressionSyntax expressionBody)
        {
            return SimpleLambdaExpression(
                    Parameter(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.UnderscoreToken,
                            "_",
                            "_",
                            TriviaList())))
                .WithExpressionBody(expressionBody);
        }
    }
}