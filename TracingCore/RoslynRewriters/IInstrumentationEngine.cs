using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore.RoslynRewriters
{
    public interface IInstrumentationEngine
    {
        CompilationUnitSyntax Start(CompilationUnitSyntax root);
    }
}