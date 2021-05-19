using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpOptBackend.SyntaxComposers
{
    public class ArgumentsList
    {
        public ArgumentListSyntax CreateArgumentList(params ExpressionSyntax[] arguments)
        {
            var args = arguments.Skip(1);
            var syntaxList = new SyntaxNodeOrTokenList().Add(Argument(arguments[0]));

            var list = args.Aggregate(syntaxList,
                (acc, cur) => acc.Add(Token(SyntaxKind.CommaToken)).Add(Argument(cur))
            );

            return ArgumentList(
                SeparatedList<ArgumentSyntax>(list)
            );
        }

        public ArgumentListSyntax CreateNamedArgumentList(params (ExpressionSyntax expr, string name)[] namedArguments)
        {
            var args = namedArguments.Skip(1);

            var (firstExpr, name) = namedArguments[0];

            var firstArg = firstExpr.ToNamedArgument(name);
            var syntaxList = new SyntaxNodeOrTokenList().Add(firstArg);

            var list = args.Aggregate(syntaxList,
                (acc, cur) => 
                    acc.Add(Token(SyntaxKind.CommaToken)).Add(cur.expr.ToNamedArgument(cur.name))
            );

            return ArgumentList(
                SeparatedList<ArgumentSyntax>(list)
            );
        }
    }
}