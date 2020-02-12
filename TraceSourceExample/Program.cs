using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore;
using TracingCore.Interceptors;
using TracingCore.JsonMappers;
using TracingCore.SourceCodeInstrumentation;
using TracingCore.TreeRewriters;
using ThreadState = System.Threading.ThreadState;

namespace TraceSourceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // var reader = new ConsoleReader();
            // var oldStream = Console.In;
            //
            // Console.SetIn(reader);
            // var a = Console.ReadLine();
            //
            // Console.WriteLine(a);
            //
            // // Console.SetIn(oldStream);
            var code = Codes.GetSimpleExample();


            var parsedTest = CSharpSyntaxTree.ParseText("var a = 5; var x = Console.ReadLine();",
                CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));
            var comp = CSharpCompilation.CreateScriptCompilation("test", parsedTest);
            var sm = comp.GetSemanticModel(parsedTest);
            var statements = parsedTest.GetCompilationUnitRoot().DescendantNodes().OfType<FieldDeclarationSyntax>();
            // var df = sm.AnalyzeDataFlow(statements.First(), statements.Last());


            var consoleHandler = new ConsoleHandler();
            TraceApi.Init(new TraceApiManager("dummy code", consoleHandler));
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetCompilationUnitRoot();
            var tracingCoreIdentifier =
                SyntaxFactory.IdentifierName("TracingCore").WithLeadingTrivia(SyntaxFactory.Space);
            var variableDataDirective =
                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("TracingCore"),
                    SyntaxFactory.IdentifierName("Data"));

            root = root.WithUsings(root.Usings
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

            TraceApi.RegisterClasses(root);
            var instrumentation = new Instrumentation(new ExpressionGenerator());
            root = instrumentation.Start(root);

            instrumentation.WriteToFile(root);

            var name = "memory";
            var compiler = new Compiler(name, root.SyntaxTree);
            var compilation = compiler.Compilation;
            var executionManager = new ExecutionManager(compilation);
            var compilationResult = executionManager.CompileAssembly(root);
            // executionManager.StartMain(compilationResult, root);

            TraceApi.RegisterEvent(data =>
            {
                var json = PyTutorDataMapper.ToJson(data);
                Console.WriteLine(json);
                
                consoleHandler.AddToRead("Marin");
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                TraceApi.ContinueExecution();

                Thread.CurrentThread.Join(5000);
            });

            var thread = executionManager.StartMainInNewThread(compilationResult, root);
            
            TraceApi.FlushPyTutorData();
            
            Console.WriteLine($"MAIN: {Thread.CurrentThread.ManagedThreadId}");

            // TraceSource ts = new TraceSource("TraceTest");

            // var consoleTraceListener = new ConsoleTraceListener();
            // // consoleTraceListener.Writer = new StreamWriter(new MemoryStream());
            // int idxConsole = ts.Listeners.Add(consoleTraceListener);
            // SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
            // ts.Switch = sourceSwitch;
            // ts.Listeners[idxConsole].Name = "console";
            // ts.Listeners["console"].TraceOutputOptions |= TraceOptions.Callstack;
            // ts.Listeners["console"].TraceOutputOptions |= TraceOptions.LogicalOperationStack;
            // ts.TraceData(TraceEventType.Verbose, 1, new object[] {"test1", "test2"});
            //
            //
            // // WriteEnvironmentInfoToTrace();
            // Console.WriteLine("Number of listeners = " + ts.Listeners.Count);
            // foreach (TraceListener traceListener in ts.Listeners)
            // {
            //     Console.Write("TraceListener: " + traceListener.Name + "\t");
            //     // The following output can be used to update the configuration file.
            //     Console.WriteLine("AssemblyQualifiedName = " +
            //                       (traceListener.GetType().AssemblyQualifiedName));
            // }
        }
    }
}