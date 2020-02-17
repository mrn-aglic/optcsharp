using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
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
        public Compiler Compiler { get; }
        public ConsoleHandler ConsoleHandler { get; }
        public ExecutionManager ExecutionManager { get; }
        public PyTutorDataManager PyTutorDataManager { get; }
        public InstrumentationConfig InstrumentationConfig { get; }

        public OptBackend(string code, IList<string> rawInputs, InstrumentationConfig instrumentationConfig)
        {
            Code = code;
            InstrumentationConfig = instrumentationConfig;
            Compiler = new Compiler();
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

        public CompilationUnitSyntax AddInstrumentation(CompilationUnitSyntax originalRoot)
        {
            var root = AddUsings(originalRoot);

            var instrumentation = new Instrumentation(new ExpressionGenerator());
            return instrumentation.Start(root);
        }

        public CompilationResult Compile(string compilationName, bool instrument)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Code);
            var originalRoot = syntaxTree.GetCompilationUnitRoot();
            var root = instrument ? AddInstrumentation(originalRoot) : originalRoot;

            Instrumentation.WriteToFile(root);

            var compilation = Compiler.Compile(compilationName, root.SyntaxTree, Compiler.DefaultCompilationOptions);
            var executionManager = new ExecutionManager();

            return new CompilationResult
            (
                executionManager.CompileAssembly(compilation),
                originalRoot,
                instrument ? root : null,
                compilation
            );
        }

        private SemanticModel GetSemanticModel(CompilationUnitSyntax root)
        {
            var syntaxTree = root.SyntaxTree;
            var compilation = Compiler.Compile("user-source", syntaxTree, Compiler.DefaultCompilationOptions);
            return compilation.GetSemanticModel(syntaxTree);
        }

        public PyTutorData Trace(CompilationUnitSyntax root, CompilationResult compilationResult)
        {
            var semanticModel = GetSemanticModel(root);
            var classManager = new ClassManager(semanticModel, new Dictionary<string, ClassData>());

            TraceApi.Init(root, new TraceApiManager(PyTutorDataManager, ConsoleHandler, classManager));
            PyTutorData pyTutorData;
            try
            {
                ExecutionManager.StartMain(compilationResult, root);
            }
            catch (ExitExecutionException)
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