using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TracingCore
{
    public class Compiler
    {
        public CSharpCompilationOptions DefaultCompilationOptions =>
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(OptimizationLevel
                .Debug);

        public CSharpCompilation Compile(string compilationName, SyntaxTree syntaxTree,
            CSharpCompilationOptions compilationOptions)
        {
            var path = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
            return CSharpCompilation
                .Create(compilationName, new[] {syntaxTree})
                .WithReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<int>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Decoder).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(path, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(typeof(TraceApi).Assembly.Location)
                )
                .WithOptions(compilationOptions);
        }
    }
}