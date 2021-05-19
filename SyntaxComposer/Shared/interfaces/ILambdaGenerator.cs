using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxComposer.Shared.interfaces
{
    public interface ILambdaGenerator
    {
        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ExpressionSyntax expressionBody);
        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ExpressionStatementSyntax expressionStatement);
        public LambdaExpressionSyntax CreateSimpleLambdaExpression(ReturnStatementSyntax returnStatementSyntax);
    }
}