using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpAnalyzer;
using CSharpOptBackend.DebuggerCommands;
using CSharpOptBackend.Rewriters;
using CSharpOptBackend.TreeDetails.HighlightManagers;
using CSharpOptBackend.TreeDetails.Interfaces;
using CSharpOptBackend.TreeManagers;
using CSharpOptBackend.TreeManagers.Factories;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyntaxComposer;
using SyntaxComposer.MessagePassing;
using SyntaxComposer.Shared;
using TracingCore;
using TracingCore.Common;
using TracingCore.JsonMappers;
using TracingCore.RoslynRewriters;
using TracingCore.TreeRewriters;

namespace TraceSourceExample
{
    class Program
    {
        private static IConfiguration LoadConfiguration()
        {
            var path = AppContext.BaseDirectory.Substring(0,
                AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));

            var builder =
                new ConfigurationBuilder()
                    .SetBasePath(path)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        public static T Test<T>(Func<T> f)
        {
            return f();
        }

        public static void Test(Action f)
        {
            f();
        }

        class TestClass
        {
        }
        
        static class TestClassProxy
        {
            // public static void Print(this TestClass instance)
            // {
            //     Console.WriteLine("PRINT");
            // }
        }

        static void Main(string[] args)
        {
            Console.WriteLine(args.GetType());

            var solutionPath = "/Users/marinagliccuvic/RiderProjects/OptLocal/CSharpAnalyzer/projects/Test/Test.sln";

            // var projectLoader = new ProjectLoader();
            // var documents = projectLoader.LoadSolution(solutionPath).Result;

            // Test(() => Console.WriteLine("Hello world"));
            // Test(() => Test(() => new List<int>()).ToArray()).ToList().ToArray();

            // var a = new List<int> {1, 2, 3, 4};
            // var b = a.ToArray();
            //
            // var c = Test(() => a);
            //
            // var t = new TestClass();
            
            // Console.WriteLine(a == b.ToList());
            
            // Func<int> a = () => 4 + 5;
            // Test((Expression<Func<int>>)a);

            // return;

            var file = "objectmessagepassing.txt";
            // var code = Codes.GetFileContents("OOP", file);
            var code = Codes.GetFileContents("", "For_2.txt");


            var options = new Dictionary<SyntaxKind, IHighlightManager>
            {
                {SyntaxKind.MethodDeclaration, new MethodHighlightManager()}
            };
            var backend = new CSharpOptBackend.OptBackend(code);

            var syntaxTree = backend.UserSyntaxTree;

            var debugStatementManager = new DebugStatementManager(new ScopeManager());
            var cur = syntaxTree.GetCompilationUnitRoot();
            var statements = debugStatementManager.CreateDebugStatements(cur);

            
            
            foreach (var statement in statements)
            {
                Console.WriteLine(statement.GetStepCommand());
            }
            
            // backend.GetDebuggerCommands(new DebugStatementManager());
            
            // var factory = new NodeDetailsFactory(options);
            // var details = backend.GetNodeDetails(new DetailsManager(factory));
            //
            // details.ToList().ForEach(x =>
            //     Console.WriteLine($"{x.Node.GetText()}\n" +
            //                       $"{x.HighlightSpans.Aggregate("", (x, y) => $"{x}\n{y}")}"));
            //
            // var methodWrappers = new MethodWrappers(new LambdaGenerator(),
            //     new ArgumentListGenerator(), new BaseMethods());
            // var composerInstance = new ComposerInstance(methodWrappers);
            //
            // var rewritten = new CSharpRewriter(composerInstance);
            //
            // rewritten.Rewrite(backend.UserSyntaxTree);
            //
            // var t1 = new TestClass();
            // object t2 = t1;
            //
            // Console.WriteLine(t2.GetType());
            
            return;

            // var code = Codes.GetStructExample();
            // var code = Codes.GetOOPExample("2_params");
            // var code = Codes.GetStructExample();
            // var code = Codes.GetForExample(1);
            // var code = Codes.GetWhileExample(1);
            // var code = Codes.GetDoWhileExample(1);
            // var code = Codes.GetMethodsExample();
            // var code = Codes.GetOOPExample("3_enkapsulacija");
            // var code = Codes.GetIfElseExample();
            // var code = Codes.GetNestedIfsExample();
            // var code = Codes.GetOOPExample("4_pikado");
            // var code = Codes.GetPropertiesExample();
            // var code = Codes.GetSimpleClassInstanceExample();
            // var code = Codes.GetClassEmptyConstructorInstanceExample();

            var config = LoadConfiguration();
            var appConfig = config.GetSection("Instrumentation").Get<InstrumentationConfig>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(appConfig);
            // serviceCollection.AddTransient<OptBackend>();
            var provider = serviceCollection.BuildServiceProvider();

            var service = provider.GetService<InstrumentationConfig>();

            PyTutorStepMapper.RegisterConfig(service);


            var optBackend = new OptBackend(code, new List<string>());
            var originalSourceCompilation = optBackend.Compile("user-code");
            var userSemanticModel =
                originalSourceCompilation.GetSemanticModel();


            var test = new SourceRewriter(new ExpressionGenerator(userSemanticModel), service);
            var insManager = new InstrumentationManager(test);
            var result = insManager.Start(userSemanticModel.SyntaxTree.GetCompilationUnitRoot()).NormalizeWhitespace();

            FileIO.WriteToFile(result.ToFullString(), "Instrumentation", "code2.txt");


            var sourceRewriter = new SourceCodeRewriter(new ExpressionGenerator(userSemanticModel), service);
            var newTree = sourceRewriter.Start(optBackend.UserSyntaxTree.GetCompilationUnitRoot());


            // return;
            FileIO.WriteToFile(newTree.ToFullString(), "Instrumentation", "code.txt");


            var instrumentationManager = new InstrumentationManager(sourceRewriter);
            var compilationResult = optBackend.Compile("opt-compilation", instrumentationManager);
            var pyTutorData = optBackend.Trace(compilationResult, true);
            // TraceApi.FlushPyTutorData();
        }
    }
}