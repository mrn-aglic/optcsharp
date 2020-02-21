using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore.RoslynRewriters
{
    public class InstrumentationManager
    {
        private readonly IInstrumentationEngine _instrumentationEngine;

        public InstrumentationManager(IInstrumentationEngine instrumentationEngine)
        {
            _instrumentationEngine = instrumentationEngine;
        }

        private CompilationUnitSyntax AddUsings(CompilationUnitSyntax root)
        {
            var tracingCoreIdentifier =
                SyntaxFactory.IdentifierName("TracingCore").WithLeadingTrivia(SyntaxFactory.Space);

            var variableDataDirective =
                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("TracingCore"),
                    SyntaxFactory.IdentifierName("TraceToPyDtos"));

            return root.WithUsings(root.Usings
                .Add(
                    SyntaxFactory.UsingDirective(
                        tracingCoreIdentifier
                    )
                )
                .Add(
                    SyntaxFactory.UsingDirective(
                        variableDataDirective
                    )
                )
            );
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            var newRoot = _instrumentationEngine.Start(root);
            return AddUsings(newRoot).NormalizeWhitespace();
        }
    }
}