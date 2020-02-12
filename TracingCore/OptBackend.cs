using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Data;
using TracingCore.Interceptors;
using TracingCore.SourceCodeInstrumentation;
using TracingCore.TraceToPyDtos;
using TracingCore.TracingManagers;
using TracingCore.TreeRewriters;

namespace TracingCore
{
    public class OptBackend
    {
        public string Code { get; }
        public ConsoleHandler ConsoleHandler { get; }
        public ExecutionManager ExecutionManager { get; }
        public PyTutorDataManager PyTutorDataManager { get; }
        public OptBackend(string code, IList<string> rawInputs)
        {
            Code = code;
            ConsoleHandler = new ConsoleHandler();
            ExecutionManager = new ExecutionManager();
            PyTutorDataManager = new PyTutorDataManager(code);
            ConsoleHandler.AddRangeToRead(rawInputs);
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

        public (CompilationUnitSyntax, SyntaxTree) AddInstrumentation(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Code);
            var originalRoot = syntaxTree.GetCompilationUnitRoot();
            var root = AddUsings(originalRoot);

            var instrumentation = new Instrumentation(new ExpressionGenerator());
            return (originalRoot, instrumentation.Start(root).SyntaxTree);
        }

        public CompilationResult Compile(bool writeToFile = false)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Code);
            var originalRoot = syntaxTree.GetCompilationUnitRoot();

            var root = AddUsings(originalRoot);

            var instrumentation = new Instrumentation(new ExpressionGenerator());
            root = instrumentation.Start(root);

            if (writeToFile)
            {
                instrumentation.WriteToFile(root);
            }

            var compilationName = "opt-compilation";
            var compiler = new Compiler(compilationName, root.SyntaxTree);
            var compilation = compiler.Compilation;

            var executionManager = new ExecutionManager();
            return new CompilationResult(executionManager.CompileAssembly(compilation), originalRoot, compilation);
        }

        public PyTutorData Trace(CompilationUnitSyntax root, CompilationResult compilationResult)
        {
            var semanticModel = compilationResult.Compilation.GetSemanticModel(root.SyntaxTree);
            var classManager = new ClassManager(new Dictionary<string, ClassData>(), semanticModel);

            TraceApi.Init(root, new TraceApiManager(PyTutorDataManager, ConsoleHandler, classManager));
            PyTutorData pyTutorData;
            try
            {
                ExecutionManager.StartMain(compilationResult, root);
            }
            catch (ExitExecutionException e)
            {
                PyTutorDataManager.RegisterRawData();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                pyTutorData = PyTutorDataManager.GetData();
            }

            return pyTutorData;
        }
    }
}