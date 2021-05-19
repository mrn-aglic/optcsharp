using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxComposer.MessagePassing.interfaces
{
    public interface IMethodWrappers
    {
        public ExpressionSyntax CreateInvocationWrapper(string invocationName, InvocationExpressionSyntax invocationExpressionSyntax);
        public ReturnStatementSyntax CreateReturnWrapper(string invocationName, ReturnStatementSyntax returnStatementSyntax);
    }
}