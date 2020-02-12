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
            
        }
    }
}