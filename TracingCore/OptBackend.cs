using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Data;
using TracingCore.Interceptors;
using TracingCore.RoslynRewriters;
using TracingCore.TraceToPyDtos;
using TracingCore.TracingManagers;

namespace TracingCore
{
    public class OptBackend
    {
        public string Code { get; }
        public Compiler Compiler { get; }
        public ConsoleHandler ConsoleHandler { get; }
        public ExecutionManager ExecutionManager { get; }
        public PyTutorDataManager PyTutorDataManager { get; }
        private readonly InstrumentationManager _instrumentationManager;

        public OptBackend
        (
            string code,
            IList<string> rawInputs,
            InstrumentationManager instrumentationManager
        )
        {
            _instrumentationManager = instrumentationManager;
            Code = code;
            Compiler = new Compiler();
            ConsoleHandler = new ConsoleHandler();
            ExecutionManager = new ExecutionManager();
            PyTutorDataManager = new PyTutorDataManager(code);

            ConsoleHandler.AddRangeToRead(rawInputs);
        }

        public CompilationUnitSyntax InstrumentSourceCode(CompilationUnitSyntax originalRoot)
        {
            return _instrumentationManager.Start(originalRoot);
        }

        public CompilationResult Compile(string compilationName, bool instrument)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(Code);
            var originalRoot = syntaxTree.GetCompilationUnitRoot();
            var root = instrument ? InstrumentSourceCode(originalRoot) : originalRoot;

            // Instrumentation.WriteToFile(root);

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
                ConsoleHandler.RestoreDefaults();
                pyTutorData = PyTutorDataManager.GetData();
            }

            return pyTutorData;
        }
    }
}