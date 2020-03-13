using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore
{
    public class CompilationResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public bool Success { get; }

        public Assembly Assembly { get; }

        public CompilationUnitSyntax Root { get; }
        public CSharpCompilation Compilation { get; }

        public CompilationResult
        (
            ImmutableArray<Diagnostic> diagnostics,
            bool success,
            Assembly assembly
        )
        {
            Diagnostics = diagnostics;
            Success = success;
            Assembly = assembly;
        }

        public CompilationResult
        (
            CompilationResult compilationResult,
            CompilationUnitSyntax root,
            CSharpCompilation compilation
        )
        {
            Diagnostics = compilationResult.Diagnostics;
            Success = compilationResult.Success;
            Assembly = compilationResult.Assembly;
            Root = root;
            Compilation = compilation;
        }

        public SemanticModel GetSemanticModel()
        {
            return Compilation.GetSemanticModel(Root.SyntaxTree);
        }
    }
}