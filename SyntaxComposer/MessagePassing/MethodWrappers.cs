using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxComposer.MessagePassing.interfaces;
using SyntaxComposer.Shared.interfaces;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SyntaxComposer.MessagePassing
{
    public class MethodWrappers : IMethodWrappers
    {
        private readonly ILambdaGenerator _lambdaGenerator;
        private readonly IArgumentListGenerator _argumentListGenerator;
        private readonly IBaseMethods _baseMethods;

        public MethodWrappers
        (
            ILambdaGenerator lambdaGenerator,
            IArgumentListGenerator argumentListGenerator,
            IBaseMethods baseMethods
        )
        {
            _lambdaGenerator = lambdaGenerator;
            _argumentListGenerator = argumentListGenerator;
            _baseMethods = baseMethods;
        }

        private InvocationExpressionSyntax CreateInvocationExpression(string name,
            ArgumentListSyntax argumentListSyntax)
        {
            return InvocationExpression(IdentifierName(name)).WithArgumentList(argumentListSyntax);
        }

        private ReturnStatementSyntax CreateReturnStatement(string name, ArgumentListSyntax argumentListSyntax)
        {
            return ReturnStatement(CreateInvocationExpression(name, argumentListSyntax));
        }

        public ExpressionSyntax CreateInvocationWrapper(string invocationName, InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var lambda = _lambdaGenerator.CreateSimpleLambdaExpression(invocationExpressionSyntax);
            var arguments = _argumentListGenerator.CreateNamedArgumentList(
                (lambda, ""),
                (_baseMethods.GetContext(invocationExpressionSyntax), "sender")
            );

            return CreateInvocationExpression(invocationName, arguments);
        }

        public ReturnStatementSyntax CreateReturnWrapper(string invocationName, ReturnStatementSyntax returnStatementSyntax)
        {
            var lambda = _lambdaGenerator.CreateSimpleLambdaExpression(returnStatementSyntax);
            var arguments = _argumentListGenerator.CreateNamedArgumentList(
                (lambda, ""),
                (_baseMethods.GetContext(returnStatementSyntax), "sender")
            );

            return CreateReturnStatement(invocationName, arguments);
        }
    }
}