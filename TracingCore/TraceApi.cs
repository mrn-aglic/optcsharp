using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Data;
using TracingCore.TraceToPyDtos;

namespace TracingCore
{
    public class TraceApi
    {
        private static TraceApiManager _traceApiManager;
        
        public static void Init(CompilationUnitSyntax root, TraceApiManager traceApiManager)
        {
            _traceApiManager = traceApiManager;
            RegisterClasses(root);
        }

        public static void TraceMethodEntry(int line, string funcName, params VariableData[] variables)
        {
            _traceApiManager.AddMethodEntry(line, funcName, variables);
        }
        
        public static void TraceData(int line, string statement, params VariableData[] variables)
        {
            _traceApiManager.TraceData(line, statement, variables);
        }
        
        public static void TraceData(int line, string statement)
        {
            TraceData(line, statement, new VariableData[0]);
        }

        public static void TraceMethodExit(int line, string statement, VariableData variableData)
        {
            _traceApiManager.AddMethodExit(line, variableData);
        }

        public static void TraceMethodReturnExit(int line, string statement, VariableData variableData)
        {
            _traceApiManager.AddMethodExit(line, variableData, false);
        }

        public static void RegisterClasses(CompilationUnitSyntax root)
        {
            _traceApiManager.RegisterClasses(root);
        }

        public static void RegisterClassLoad(string fullyQualifiedName)
        {
            _traceApiManager.RegisterClassLoad(fullyQualifiedName);
        }
        
        public static void FlushPyTutorData()
        {
            _traceApiManager.FlushPyTutorData();
        }
    }
}