using System;
using System.Collections.Generic;
using TracingCore;

namespace TraceSourceExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args.GetType());
            var code = Codes.GetPropertiesExample();

            var optBackend = new OptBackend(code, new List<string>());

            var compilationResult = optBackend.Compile("opt-compilation", true);
            var pyTutorData = optBackend.Trace(compilationResult.Root, compilationResult);
            
            TraceApi.FlushPyTutorData();
        }
    }
}