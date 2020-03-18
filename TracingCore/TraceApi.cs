using TracingCore.TraceToPyDtos;

namespace TracingCore
{
    public class TraceApi
    {
        private static TraceApiManager _traceApiManager;

        public static void Init(TraceApiManager traceApiManager)
        {
            _traceApiManager = traceApiManager;
            RegisterClasses();
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
            return; // TODO: Finish implemntation
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

        public static void ConditionalTrace(int line, bool conditionResult, params VariableData[] variables)
        {
            _traceApiManager.ConditionalTrace(line, conditionResult, variables);
        }

        public static void RegisterIteration(string keyword, string location)
        {
            _traceApiManager.RegisterLoopIteration(keyword, location);
        }

        public static void TraceDataWithStatementHits
        (
            int line,
            string statement,
            string location,
            params VariableData[] variables
        )
        {
        }

        public static void TraceMethodExit(int line, bool replacePrev)
        {
            _traceApiManager.AddMethodExit(line, null, replacePrev);
        }

        public static void TraceMethodReturnExit(int line, VariableData variableData)
        {
            _traceApiManager.AddMethodExit(line, variableData, false);
        }

        public static void RegisterClasses()
        {
            _traceApiManager.RegisterClasses();
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