using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore.RoslynRewriters
{
    public class InstrumentationManager
    {
        private static readonly IdentifierNameSyntax TracingCoreIdentifierName =
            SyntaxFactory.IdentifierName("TracingCore");

        private static readonly IdentifierNameSyntax TraceApiNamespace = TracingCoreIdentifierName
            .WithLeadingTrivia(SyntaxFactory.Space);

        private static readonly QualifiedNameSyntax TraceDtosNamespace = SyntaxFactory.QualifiedName(
            TracingCoreIdentifierName,
            SyntaxFactory.IdentifierName("TraceToPyDtos"));

        private readonly IInstrumentationEngine _instrumentationEngine;

        public InstrumentationManager(IInstrumentationEngine instrumentationEngine)
        {
            _instrumentationEngine = instrumentationEngine;
        }

        private CompilationUnitSyntax AddUsings(CompilationUnitSyntax root)
        {
            return root.WithUsings(root.Usings
                .Add(
                    SyntaxFactory.UsingDirective(
                        TraceApiNamespace
                    )
                )
                .Add(
                    SyntaxFactory.UsingDirective(
                        TraceDtosNamespace
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