using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using TracingCore.Common;

namespace TracingCore
{
    public class ExecutionManager
    {
        public const string MainName = "Main";

        public BindingFlags MainMethodBindingFlags =
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod;

        public CompilationResult CompileAssembly(CSharpCompilation compilation)
        {
            using (var stream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(stream);
                if (result.Success)
                {
                    var assembly = Assembly.Load(stream.GetBuffer());
                    return new CompilationResult(result.Diagnostics, true, assembly);
                }

                result.Diagnostics.ToList().ForEach(Console.WriteLine);
                return new CompilationResult(result.Diagnostics, false, null);
            }
        }

        private string GetMainFullName(CompilationUnitSyntax root)
        {
            var mainMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(x => x.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)) && x.Identifier.Text == MainName);

            if (mainMethod == null)
            {
                throw new MissingMethodException("Main method not found!");
            }

            var @class = (ClassDeclarationSyntax) mainMethod.Parent;
            return RoslynHelper.GetClassParentPath(@class);
        }

        private MethodInfo LoadMethod(CompilationResult compilationResult, string instanceName)
        {
            var assembly = compilationResult.Assembly;
            Type type = assembly.GetType(instanceName);
            var method = type.GetMethods(MainMethodBindingFlags)
                .FirstOrDefault(x => x.Name == MainName);
            return method;
        }

        public void StartMain(CompilationResult compilationResult, CompilationUnitSyntax root)
        {
            var instanceName = GetMainFullName(root);
            var method = LoadMethod(compilationResult, instanceName);
            InvokeMethod(method);
        }

        private void InvokeMethod(MethodInfo method)
        {
            var @params = method.GetParameters().Length == 0 ? new object?[0] : new object?[] {new string[1]};
            method.Invoke(null, @params);
        }
    }
}