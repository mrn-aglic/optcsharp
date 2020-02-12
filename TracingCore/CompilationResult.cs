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
        public CompilationUnitSyntax NewRoot { get; }
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
            Root = null;
        }
        
        public CompilationResult
        (
            CompilationResult compilationResult,
            CompilationUnitSyntax root,
            CompilationUnitSyntax newRoot,
            CSharpCompilation compilation
        )
        {
            Diagnostics = compilationResult.Diagnostics;
            Success = compilationResult.Success;
            Assembly = compilationResult.Assembly;
            Root = root;
            NewRoot = newRoot;
            Compilation = compilation;
        }
    }
}