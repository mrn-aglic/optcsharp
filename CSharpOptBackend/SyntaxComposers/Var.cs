using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpOptBackend.SyntaxComposers
{
    public class Var
    {
        public static ExpressionSyntax GetVar(string name)
        {
            return ObjectCreationExpression(
                IdentifierName("Var")
            ).WithArgumentList(
                ArgumentList.CreateArgumentList(
                    Expression.GetLiteralExpression(name),
                    IdentifierName(name)
                )
            );
        }
    }
}