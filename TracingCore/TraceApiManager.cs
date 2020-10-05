using System.Collections.Generic;
using System.Linq;
using TracingCore.Data;
using TracingCore.Interceptors;
using TracingCore.TraceToPyDtos;
using TracingCore.TracingManagers;

namespace TracingCore
{
    public class TraceApiManager
    {
        private readonly PyTutorDataManager _pyTutorDataManager;
        private readonly ConsoleHandler _consoleHandler;
        private readonly ClassManager _classManager;
        private readonly StructManager _structManager;
        private readonly LoopManager _loopManager;

        public TraceApiManager(PyTutorDataManager pyTutorDataManager, ConsoleHandler consoleHandler,
            ClassManager classManager, StructManager structManager, LoopManager loopManager)
        {
            _pyTutorDataManager = pyTutorDataManager;
            _consoleHandler = consoleHandler;
            _classManager = classManager;
            _structManager = structManager;
            _loopManager = loopManager;
        }

        public void TraceData(int line, params VariableData[] variables)
        {
            var stdOut = _consoleHandler.GetConsoleOutput();
            _pyTutorDataManager.AddNextTraceEntry(line, stdOut, variables.ToList());
        }

        public void AddMethodEntry(int line, string methodName, IList<VariableData> parameters)
        {
            _pyTutorDataManager.AddMethodEntry(line, methodName, parameters);
        }

        public void AddMethodExit(int line, VariableData variableData, bool replacePrev = true)
        {
            var varData = variableData ?? VariableData.EmptyVariableData();
            _pyTutorDataManager.AddMethodExit(line, varData, replacePrev);
        }

        public void RegisterClassLoad(string fullyQualifiedName)
        {
            var classData = _classManager.GetClassData(fullyQualifiedName);
            _pyTutorDataManager.RegisterClass(classData);
        }

        public void FlushPyTutorData()
        {
            _pyTutorDataManager.FlushPyTutorData();
        }

        public void RegisterLoopIteration(string keyword, string location)
        {
            _loopManager.UpdateLoopIteration(keyword, location);
        }

        public int GetLoopIteration(string keyword, string location)
        {
            return _loopManager.GetLoopIteration(keyword, location);
        }

        public void ConditionalTrace(int line, bool expressionResult, VariableData[] variables)
        {
            var vars = expressionResult ? variables : new VariableData[0];
            TraceData(line, vars);
            // _pyTutorDataManager.AddNextTraceEntry(line, stdOut, new List<VariableData>());
        }

        public void Init()
        {
            _consoleHandler.Init();
            _classManager.RegisterClasses();
            _structManager.RegisterStructs();

            var structs = _structManager.GetStructData();
            _pyTutorDataManager.PreloadHeap(structs.Select(x => x as TypeDeclarationData).ToList());
        }

        public void Reset()
        {
            _consoleHandler.RestoreDefaults();
        }
    }
}