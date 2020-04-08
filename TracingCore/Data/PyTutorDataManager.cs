using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using TracingCore.Common;
using TracingCore.JsonMappers;
using TracingCore.TraceToPyDtos;

namespace TracingCore.Data
{
    using StacksToRender = IImmutableStack<FuncStack>;
    using Heap = ImmutableDictionary<int, HeapData>;

    public class PyTutorDataManager
    {
        private const string FunctionCallEvent = "call";
        private const string StepLineEvent = "step_line";
        private const string RawInputEvent = "raw_input";
        private const string ReturnEvent = "return";
        private const string ExceptionEvent = "exception";
        private const string UncaughtException = "uncaught_exception";

        private int _currentFrameId;
        private int _currentHeapId;
        private string _currentFunctionName = "<module>";
        private readonly PyTutorData _pyTutorData;

        // TODO: Implement
        private Dictionary<string, string> _hideVariables;

        public PyTutorDataManager(string code)
        {
            _pyTutorData = new PyTutorData(code, new List<IPyTutorStep>());
            _hideVariables = new Dictionary<string, string>();

            _currentFrameId = 1;
            _currentHeapId = 1;
        }

        public HeapData GetValueFromHeap(object value, ImmutableDictionary<int, HeapData> heap)
        {
            var dataInHeap = heap.Values.FirstOrDefault(x => x.Value == value);
            return dataInHeap;
        }

        public OptVariableData EncodedLocalFromVariableData(VariableData variableData, HeapData heapData)
        {
            // || variableData.Value == null prevents FilteredHeap from doing anything.
            var isRef = !(variableData.IsValueType || variableData.Value is string || variableData.Value == null);

            return new OptVariableData(
                variableData.Name,
                variableData.Value,
                variableData.Type.ToString(),
                isRef,
                isRef ? heapData.HeapId : (int?) null
            );
        }

        private ImmutableStack<FuncStack> GetFuncStacks()
        {
            var lastStep = _pyTutorData.Trace.LastOrDefault(x => x is PyTutorStep);
            return lastStep != null
                ? (lastStep as PyTutorStep).StackToRender
                : ImmutableStack<FuncStack>.Empty;
        }

        public FuncStack CreateFuncStack(string funcName, ImmutableList<OptVariableData> encodedLocals)
        {
            return new FuncStack(
                funcName,
                _currentFrameId,
                true,
                encodedLocals
            );
        }

        public HeapData GetValueOrDefaultFromHeap(ImmutableDictionary<int, HeapData> heap, Func<HeapData, bool> p)
        {
            foreach (var data in heap.Values)
            {
                if (p(data))
                {
                    return data as VarHeapData;
                }
            }

            return null;
        }

        public (List<VariableData>, List<(VariableData, HeapData)>) GroupByOnHeap(IList<VariableData> variables,
            Heap heap)
        {
            var notOnHeap = new List<VariableData>();
            var onHeap = new List<(VariableData, HeapData)>();

            foreach (var variable in variables)
            {
                bool Func(HeapData data) => variable.Value == data.Value;

                var hd = GetValueOrDefaultFromHeap(heap, Func);
                if (hd == null)
                {
                    notOnHeap.Add(variable);
                }
                else
                {
                    onHeap.Add((variable, hd));
                }
            }

            return (notOnHeap, onHeap);
        }

        private ImmutableDictionary<int, HeapData> UpdateHeap(IList<VariableData> variables, PyTutorStep lastStep)
        {
            var lastStepExists = lastStep == null;
            var refVars = variables.Where(OptVariableBase.IsVarRef).ToList();

            var (varsNotOnHeap, varsOnHeap) =
                lastStepExists
                    ? (variables, new List<(VariableData, HeapData)>())
                    : GroupByOnHeap(refVars, lastStep.Heap);
            var updatedHeapValues = varsOnHeap
                .Select(x => x.Item2 is VarHeapData varData ? varData.Copy(x.Item1.Value) : x.Item2)
                .ToImmutableDictionary(x => x.HeapId, x => x);

            var newHeapData = varsNotOnHeap.Select(GetHeapData).ToImmutableDictionary(x => x.HeapId, x => x);

            var prevHeap = lastStepExists ? ImmutableDictionary<int, HeapData>.Empty : lastStep.Heap;
            return prevHeap
                .RemoveRange(updatedHeapValues.Select(x => x.Key))
                .AddRange(updatedHeapValues)
                .AddRange(newHeapData);
        }

        private ImmutableDictionary<int, HeapData> UpdateHeap(IList<VariableData> variables, Heap heap)
        {
            var refVars = variables.Where(OptVariableBase.IsVarRef).ToList();

            var (varsNotOnHeap, varsOnHeap) = GroupByOnHeap(refVars, heap);
            var updatedHeapValues = varsOnHeap
                .Select(x => x.Item2 is VarHeapData varData ? varData.Copy(x.Item1.Value) : x.Item2)
                .ToImmutableDictionary(x => x.HeapId, x => x);

            var newHeapData = varsNotOnHeap.Select(GetHeapData).ToImmutableDictionary(x => x.HeapId, x => x);

            return heap
                .RemoveRange(updatedHeapValues.Select(x => x.Key))
                .AddRange(updatedHeapValues)
                .AddRange(newHeapData);
        }

        public void AddLineStep(int line, string stdOut, IList<VariableData> variables)
        {
            var lastStep = _pyTutorData.Trace.Last() as PyTutorStep;
            // var @event = lastStep.Line == line ? ReturnEvent : StepLineEvent;
            var @event = StepLineEvent;

            var (existingStacks, lheap) = IsReturnFromFunction() ? PyTutorGc() : (GetFuncStacks(), lastStep.Heap);

            // var existingStacks = GetFuncStacks();
            // if (IsReturnFromFunction())
            // {
            // existingStacks = existingStacks.Pop();
            // }

            var lastStack = existingStacks.First();

            var heap = UpdateHeap(variables, lheap);
            var encLocals = variables.Select(x => EncodedLocalFromVariableData(x, GetValueFromHeap(x.Value, heap)));

            var newStack = lastStack.AddAndUpdate(encLocals.ToImmutableList());

            var stacks = existingStacks.Pop().Push(newStack);

            var pyTutorStep = new PyTutorStep(
                line,
                @event,
                _currentFunctionName,
                stdOut,
                lastStep.Globals,
                stacks,
                heap,
                PyTutorStepMapper.HeapToJson(heap)
            );

            _pyTutorData.Trace.Add(pyTutorStep);
        }

        private bool IsReturnFromFunction()
        {
            return _pyTutorData.Trace.Last() is PyTutorStep lastStep &&
                   lastStep.Event == ReturnEvent;
        }
        
        private int RestoreKey(KeyValuePair<string, object> el)
        {
            var arr = el.Value as object[];
            if (arr == null) throw new ArgumentException("Invalid value occured in Heap");
            return (int) arr[1];
        }

        private (StacksToRender, Heap) PyTutorGc()
        {
            var lastStep = _pyTutorData.Trace.Last() as PyTutorStep;
            if (lastStep.Event != ReturnEvent)
            {
                throw new ArgumentException("Last entry was not return from function");
            }

            var stacksToRender = lastStep.StackToRender.Pop(out _);
            var currentGlobals = lastStep.Globals;
            var currentHeap = lastStep.Heap;

            var removeFromHeap = new List<int>();

            foreach (var (key, _) in currentHeap)
            {
                var anyRefInStacks = stacksToRender.Any(x => x.EncodedLocals.Any(y => y.HeapId == key));
                var anyRefInGlobals = currentGlobals.Any(x => key == RestoreKey(x));

                if (!(anyRefInGlobals || anyRefInStacks))
                {
                    removeFromHeap.Add(key);
                }
            }

            var newHeap = currentHeap.RemoveRange(removeFromHeap);

            return (stacksToRender, newHeap);
        }

        public void AddNextTraceEntry(int line, string stdOut, IList<VariableData> variables)
        {
            AddLineStep(line, stdOut, variables);
        }

        private HeapData CreateHeapDataInstance(int id, string type, object value)
        {
            return new VarHeapData
            (
                id,
                type,
                value
            );
        }

        private HeapData GetHeapData(VariableData variableData)
        {
            if (!OptVariableBase.IsVarRef(variableData))
            {
                return null;
            }

            var heapData = CreateHeapDataInstance(_currentHeapId, variableData.Type.ToString(), variableData.Value);
            _currentHeapId++;
            return heapData;
        }

        private MethodHeapData GetHeapData(MethodData methodData, string parentFullName)
        {
            var heapData = new MethodHeapData(_currentHeapId, methodData.Name, parentFullName, methodData.Declaration);
            _currentHeapId++;
            return heapData;
        }

        private ClassHeapData GetHeapData(ClassData classData)
        {
            var heapData = new ClassHeapData(
                _currentHeapId,
                classData.IsStatic,
                classData.Type,
                classData.FullyQualifiedPath,
                classData.ExtendedTypes
            );
            _currentHeapId++;
            return heapData;
        }

        private IList<VariableData> IgnoreArgsIfAllNull(string methodName, IList<VariableData> variables)
        {
            if (methodName != "Main")
            {
                return variables;
            }

            return variables.Where(x => x.Name != "args" || (x.Value as string[]).Any(y => y != null)).ToList();
        }

        public void AddMethodEntry(int line, string methodName, IList<VariableData> parameters)
        {
            parameters = IgnoreArgsIfAllNull(methodName, parameters);
            var lastStep = (PyTutorStep) _pyTutorData.Trace.LastOrDefault();

            var (existingStacks, lheap) = lastStep == null ? (GetFuncStacks(), Heap.Empty) :
                IsReturnFromFunction() ? PyTutorGc() : (GetFuncStacks(), lastStep.Heap);

            var heap = UpdateHeap(parameters, lheap);
            var encLocals = parameters.Select(x => EncodedLocalFromVariableData(x, GetValueFromHeap(x.Value, heap)));

            var funcStack = CreateFuncStack(methodName, encLocals.ToImmutableList());
            var stacks = existingStacks.Push(funcStack);

            var stdOut = lastStep != null ? lastStep.StdOut : string.Empty;
            var globals = lastStep != null ? lastStep.Globals : ImmutableDictionary<string, object>.Empty;

            var pyTutorStep = new PyTutorStep(
                line,
                FunctionCallEvent,
                methodName,
                stdOut,
                globals,
                stacks,
                heap,
                PyTutorStepMapper.HeapToJson(heap)
            );

            _pyTutorData.Trace.Add(pyTutorStep);

            _currentFunctionName = methodName;
            _currentFrameId++;
        }

        public void AddMethodExit(int line, List<VariableData> variables)
        {
        }


        public void AddMethodExit(int line, VariableData variableData, bool replacePrevStep)
        {
            var prevStep = _pyTutorData.Trace.Last() as PyTutorStep;

            if (prevStep == null) throw new ArgumentException("There should be a step before method exit");

            // var (gcStacks, _) = IsReturnFromFunction()
            //     ? PyTutorGc()
            //     : (prevStep.StackToRender.Pop(out var lastStack), null);

            var newStackToRender = prevStep.StackToRender.Pop(out var lastStack);
            var hd = GetHeapData(variableData);
            var heapId = hd?.HeapId;

            var optVariable = OptVariableData.GetReturnValue(variableData, heapId);
            var newStack = lastStack.AddAndUpdate(optVariable);

            var heap = heapId.HasValue ? prevStep.Heap.Add(hd.HeapId, hd) : prevStep.Heap;

            var newStep = new PyTutorStep(
                line,
                ReturnEvent,
                prevStep.FuncName,
                prevStep.StdOut,
                prevStep.Globals,
                newStackToRender.Push(newStack),
                heap,
                PyTutorStepMapper.HeapToJson(heap)
            );

            if (replacePrevStep || prevStep.StackToRender.Count() == 1)
            {
                _pyTutorData.Trace.Remove(prevStep);
            }

            _pyTutorData.Trace.Add(newStep);
        }

        public void RegisterClass(ClassData classData)
        {
            var lastStep = _pyTutorData.Trace.Last() as PyTutorStep;

            var heapClass = GetHeapData(classData);
            var heapMethods = classData.Methods.Select(x => GetHeapData(x, classData.FullyQualifiedPath));
            var methodsDict = heapMethods.ToImmutableDictionary(x => x.HeapId, x => x as HeapData);

            var heap = lastStep.Heap.Add(heapClass.HeapId, heapClass)
                .AddRange(methodsDict);

            var jHeap = PyTutorStepMapper.HeapToJson(heap);
            var globals = lastStep.Globals.Add(classData.Name, new object[] {"REF", heapClass.HeapId});

            var newStep = new PyTutorStep(
                classData.LineData.StartLine,
                StepLineEvent,
                lastStep.FuncName,
                lastStep.StdOut,
                globals,
                lastStep.StackToRender,
                heap,
                jHeap
            );

            _pyTutorData.Trace.Add(newStep);
        }

        private void RegisterExceptionShared(int line, string message, string @event)
        {
            var lastStep = _pyTutorData.Trace.LastOrDefault() as PyTutorStep;

            if (lastStep == null)
            {
                _pyTutorData.Trace.Add(
                    new PyTutorStep(
                        line,
                        @event,
                        string.Empty,
                        string.Empty,
                        message,
                        ImmutableDictionary<string, object>.Empty,
                        ImmutableStack<FuncStack>.Empty,
                        ImmutableDictionary<int, HeapData>.Empty,
                        new JObject()
                    )
                );
                return;
            }

            var newStep = new PyTutorStep(
                line,
                @event,
                lastStep.FuncName,
                lastStep.StdOut,
                message,
                lastStep.Globals,
                lastStep.StackToRender,
                lastStep.Heap,
                lastStep.JHeap
            );
            _pyTutorData.Trace.Add(newStep);
        }

        public void RegisterException(int line, string message)
        {
            RegisterExceptionShared(line, message, ExceptionEvent);
        }

        public void RegisterUncaughtException(int line, string message)
        {
            RegisterExceptionShared(line, message, UncaughtException);
        }

        public void RegisterRawData()
        {
            _pyTutorData.Trace.Add(new PyTutorRawInputStep("", RawInputEvent));
        }

        public void FlushPyTutorData()
        {
            var jObject = PyTutorDataMapper.ToJson(_pyTutorData);
            FileIO.WriteToFile(jObject.ToString(), "JsonOutput", "optOutput.json");
        }

        public int GetLastStepLine()
        {
            return ((PyTutorStep) _pyTutorData.Trace.Last()).Line;
        }

        public PyTutorData GetData()
        {
            return _pyTutorData;
        }
    }
}