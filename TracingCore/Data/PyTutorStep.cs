using System;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace TracingCore.Data
{
    public interface IPyTutorStep
    {
        public string Event { get; }
    }

    public class PyTutorStep : IPyTutorStep
    {
        public int Line { get; }
        public string FuncName { get; }
        public string StdOut { get; }
        public ImmutableStack<FuncStack> StackToRender { get; }
        public IImmutableDictionary<string, object> Globals;
        public IImmutableList<string> OrderedGlobals { get; }
        public ImmutableDictionary<int, HeapData> Heap { get; }
        
        public ImmutableDictionary<int, HeapData> FilteredHeap =>
            Heap.Where(x => _ignore(x.Value)).ToImmutableDictionary();

        public string Event { get; }
        
        public JObject JHeap { get; }

        private readonly Func<HeapData, bool> _ignore;

        public PyTutorStep
        (
            int lineNum,
            string @event,
            string funcName,
            string stdOut,
            IImmutableStack<FuncStack> stacks,
            ImmutableDictionary<int, HeapData> heap,
            JObject jHeap)
        {
            Line = lineNum;
            Event = @event;
            FuncName = funcName;
            StdOut = stdOut;
            StackToRender = ImmutableStack<FuncStack>.Empty;
            foreach (var stack in stacks.Reverse())
            {
                StackToRender = StackToRender.Push(stack);
            } 

            JHeap = jHeap;
            Globals = ImmutableDictionary<string, object>.Empty;
            OrderedGlobals = ImmutableList<string>.Empty;
            Heap = heap;

            _ignore = hd => hd.Value != null;
        }
        
        public PyTutorStep
        (
            int lineNum,
            string @event,
            string funcName,
            string stdOut,
            IImmutableDictionary<string, object> globals,
            IImmutableStack<FuncStack> stacks,
            ImmutableDictionary<int, HeapData> heap,
            JObject jHeap)
        {
            Line = lineNum;
            Event = @event;
            FuncName = funcName;
            StdOut = stdOut;
            StackToRender = ImmutableStack<FuncStack>.Empty;
            foreach (var stack in stacks.Reverse())
            {
                StackToRender = StackToRender.Push(stack);
            } 

            JHeap = jHeap;
            Globals = globals;
            OrderedGlobals = globals.Select(x => x.Key).ToImmutableList();
            Heap = heap;

            _ignore = hd => hd.Value != null;
        }
    }

    public class PyTutorRawInputStep : IPyTutorStep
    {
        public string Prompt { get; }
        public string Event { get; }

        public PyTutorRawInputStep(string prompt, string @event)
        {
            Prompt = prompt;
            Event = @event;
        }
    }
}