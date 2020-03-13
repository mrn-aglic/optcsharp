using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static void Clear()
        {
            _traceApiManager = null;
        }

        public static void TraceMethodEntry(int line, string funcName, params VariableData[] variables)
        {
            _traceApiManager.AddMethodEntry(line, funcName, variables);
        }

        public static void TraceBlockEntry(int line, params VariableData[] variables)
        {
            // TODO implement traceBlockEntry and exit
            _traceApiManager.TraceData(line, variables);
        }

        public static void TraceBlockExit(int line)
        {
            return;
            _traceApiManager.TraceData(line);
        }

        public static void TraceData(int line)
        {
            TraceData(line, new VariableData[0]);
        }

        public static void TraceData(int line, params VariableData[] variables)
        {
            _traceApiManager.TraceData(line, variables);
        }

        public static void TraceMethodExit(int line, bool replacePrev)
        {
            _traceApiManager.AddMethodExit(line, null, replacePrev);
        }

        public static void TraceMethodReturnExit(int line, VariableData variableData)
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