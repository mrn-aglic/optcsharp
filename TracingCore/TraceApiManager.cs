using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
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
        public TraceApiManager(PyTutorDataManager pyTutorDataManager, ConsoleHandler consoleHandler, ClassManager classManager)
        {
            _pyTutorDataManager = pyTutorDataManager;
            _consoleHandler = consoleHandler;
            _classManager = classManager;
        }

        public void TraceData(int line, string statement, params VariableData[] variables)
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

        public void RegisterClasses(CompilationUnitSyntax root)
        {
            _classManager.RegisterClasses(root);
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
    }
}