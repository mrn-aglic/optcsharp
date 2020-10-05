using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        static void Main(string[] args)
        {
            Console.WriteLine(args.GetType());
            var code = Codes.GetStructExample();
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