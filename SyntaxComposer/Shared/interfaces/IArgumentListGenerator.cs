using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxComposer.Shared.interfaces
{
    public interface IArgumentListGenerator
    {
        public ArgumentListSyntax CreateArgumentList(params ExpressionSyntax[] arguments);
        public ArgumentListSyntax CreateNamedArgumentList(params (ExpressionSyntax expr, string name)[] namedArguments);
    }
}