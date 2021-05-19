using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpOptBackend.SyntaxComposers
{
    public class Expression
    {
        public static ExpressionSyntax GetLiteralExpression(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        }

        public static ExpressionSyntax GetLiteralExpression(int value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        }

        public static InvocationExpressionSyntax GetMemberInvocationExpression(string context, string member)
        {
            return InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(context),
                    IdentifierName(member)
                )
            );
        }
    }
}