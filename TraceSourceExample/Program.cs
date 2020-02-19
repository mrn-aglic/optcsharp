using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TracingCore;
using TracingCore.JsonMappers;

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
            var code = Codes.GetMethodsExample();
            
            var config = LoadConfiguration();
            var appConfig = config.GetSection("instrumentation").Get<InstrumentationConfig>();
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(appConfig);
            // serviceCollection.AddTransient<OptBackend>();
            var provider = serviceCollection.BuildServiceProvider();

            var service = provider.GetService<InstrumentationConfig>();
            
            PyTutorStepMapper.RegisterConfig(service);
            var optBackend = new OptBackend(code, new List<string>(), service);

            var compilationResult = optBackend.Compile("opt-compilation", true);
            var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);
            
            TraceApi.FlushPyTutorData();
        }
    }
}