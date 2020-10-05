using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Data;
using TracingCore.Exceptions;
using TracingCore.Interceptors;
using TracingCore.RoslynRewriters;
using TracingCore.TraceToPyDtos;
using TracingCore.TracingManagers;

namespace TracingCore
{
    public class OptBackend
    {
        public string Code { get; }
        public SyntaxTree UserSyntaxTree { get; }
        public Compiler Compiler { get; }
        public ConsoleHandler ConsoleHandler { get; }
        public ExecutionManager ExecutionManager { get; }
        public PyTutorDataManager PyTutorDataManager { get; }

        private class OriginalLocation
        {
            public int StartLine { get; }
            public int EndLine { get; }

            public OriginalLocation(int startLine, int endLine)
            {
                StartLine = startLine;
                EndLine = endLine;
            }
        }

        private Dictionary<SyntaxNode, OriginalLocation> _locationsMap;

        public OptBackend
        (
            string code,
            IList<string> rawInputs
        )
        {
            Code = code;
            UserSyntaxTree = CSharpSyntaxTree.ParseText(code);
            Compiler = new Compiler();
            ConsoleHandler = new ConsoleHandler();
            ExecutionManager = new ExecutionManager();
            PyTutorDataManager = new PyTutorDataManager(code);

            _locationsMap = new Dictionary<SyntaxNode, OriginalLocation>();

            ConsoleHandler.AddRangeToRead(rawInputs);
        }

        public CompilationUnitSyntax InstrumentSourceCode(CompilationUnitSyntax originalRoot,
            InstrumentationManager instrumentationManager)
        {
            return instrumentationManager.Start(originalRoot);
        }

        public CompilationResult Compile(string compilationName)
        {
            var root = UserSyntaxTree.GetCompilationUnitRoot();

            var compilation = Compiler.Compile(compilationName, root.SyntaxTree, Compiler.DefaultCompilationOptions);
            var executionManager = new ExecutionManager();

            return new CompilationResult
            (
                executionManager.CompileAssembly(compilation),
                root,
                compilation
            );
        }

        public CompilationResult Compile(string compilationName, InstrumentationManager instrumentationManager)
        {
            var originalRoot = UserSyntaxTree.GetCompilationUnitRoot();

            var root = InstrumentSourceCode(originalRoot, instrumentationManager);

            // Instrumentation.WriteToFile(root);

            var compilation = Compiler.Compile(compilationName, root.SyntaxTree, Compiler.DefaultCompilationOptions);
            var executionManager = new ExecutionManager();

            return new CompilationResult
            (
                executionManager.CompileAssembly(compilation),
                root,
                compilation
            );
        }

        public PyTutorData Trace
        (
            CompilationResult compilationResult,
            bool flushData = false
        )
        {
            var classManager = new ClassManager(compilationResult, new Dictionary<string, ClassData>());
            var structManager = new StructManager(compilationResult, new Dictionary<string, StructData>());
            var loopManager = new LoopManager();
            var traceApiManager = new TraceApiManager(PyTutorDataManager, ConsoleHandler, classManager, structManager,
                loopManager);

            TraceApi.Init(traceApiManager);

            PyTutorData pyTutorData;
            try
            {
                ExecutionManager.StartMain(compilationResult, compilationResult.Root);
            }
            catch (ExitExecutionException)
            {
                PyTutorDataManager.RegisterRawData();
            }
            catch (NumIterationsException numIterationsException)
            {
                PyTutorDataManager.RegisterUncaughtException(numIterationsException.Line,
                    numIterationsException.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                PyTutorDataManager.RegisterUncaughtException(PyTutorDataManager.GetLastStepLine(), e.Message);
            }
            finally
            {
                // AssemblyLoadContext.GetLoadContext(compilationResult.Assembly).Unload();
                pyTutorData = PyTutorDataManager.GetData();
                if (flushData) PyTutorDataManager.FlushPyTutorData();
                TraceApi.Clear();
            }

            return pyTutorData;
        }

        public PyTutorData ReportCompilationError(CompilationResult compilationResult)
        {
            foreach (var resultDiagnostic in compilationResult.Diagnostics)
            {
                int line = resultDiagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;
                string msg = $"Linija: {line}, {resultDiagnostic.GetMessage()}";
                PyTutorDataManager.RegisterUncaughtException(line, msg);
            }

            return PyTutorDataManager.GetData();
        }
    }
}