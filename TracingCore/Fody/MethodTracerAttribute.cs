using System;
using System.Linq;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using TracingCore.Data;
using TracingCore.Fody;
using TracingCore.TraceToPyDtos;

[module:MethodTracer(-1)]

namespace TracingCore.Fody
{

    [AttributeUsage
        (
            AttributeTargets.Method |
            AttributeTargets.Constructor |
            AttributeTargets.Assembly |
            AttributeTargets.Module,
            AllowMultiple = true,
            Inherited = false
        )
    ]
    public class MethodTracerAttribute : MethodDecoratorAttribute, IMethodDecorator, IPartialDecoratorExit1
    {
        private readonly int _lineNumber;

        private object _instance;
        private MethodBase _methodBase;
        private VariableData[] _params;

        public MethodTracerAttribute(int lineNumber)
        {
            _lineNumber = lineNumber;
        }

        public void Init(object instance, MethodBase method, object[] args)
        {
            _instance = instance;
            _methodBase = method;
            _params = method.GetParameters().Select((p, i) =>
                new VariableData(p.Name, args[i], p.ParameterType)
            ).ToArray();
        }

        public void OnEntry()
        {
            var methodName = _methodBase.Name;
            TraceApi.TraceMethodEntry(_lineNumber, methodName, _params);
        }

        public void OnExit()
        {
        }

        public void OnException(Exception exception)
        {
        }

        public void OnExit(object iRetval)
        {
        }
    }
}