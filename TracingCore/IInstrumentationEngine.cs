using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore
{
    public interface IInstrumentationEngine
    {
        CompilationUnitSyntax Start(CompilationUnitSyntax root);
    }
}