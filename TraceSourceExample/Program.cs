using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TracingCore;
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
            var code = Codes.GetClassEmptyConstructorInstanceExample();

            var config = LoadConfiguration();
            var appConfig = config.GetSection("instrumentation").Get<InstrumentationConfig>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(appConfig);
            // serviceCollection.AddTransient<OptBackend>();
            var provider = serviceCollection.BuildServiceProvider();

            var service = provider.GetService<InstrumentationConfig>();
            
            PyTutorStepMapper.RegisterConfig(service);

            var tree = CSharpSyntaxTree.ParseText(code);
            
            var srcRew = new SourceCodeRewriter(new ExpressionGenerator(), service);
            var rewriter = new InstrumentationManager(srcRew);

            var newTree = rewriter.Start(tree.GetCompilationUnitRoot());
            
            // return;
            
            // var sourceRewriter = new SourceCodeRewriter(new ExpressionGenerator(), service);
            var optBackend = new OptBackend(code, new List<string>(),  rewriter);

            var compilationResult = optBackend.Compile("opt-compilation", true);
            var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);
            
            TraceApi.FlushPyTutorData();
        }
    }
}