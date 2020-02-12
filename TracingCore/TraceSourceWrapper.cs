using System;
using System.Diagnostics;

namespace TracingCore
{
    public class TraceSourceWrapper
    {
        public TraceSource TraceSource { get; }
        public ConsoleTraceListener ConsoleListener { get; }
        private int _id;

        public int Id => _id;

        public TraceSourceWrapper()
        {
            _id = 0;
            TraceSource = new TraceSource(TraceSourceName.RoslynTraceSource);
            ConsoleListener = new ConsoleTraceListener();
            // ConsoleListener.Writer = new StreamWriter(new MemoryStream());

            int idxConsole = TraceSource.Listeners.Add(ConsoleListener);
            SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
            TraceSource.Switch = sourceSwitch;
            TraceSource.Listeners[idxConsole].Name = "console";
            TraceSource.Listeners["console"].TraceOutputOptions |= TraceOptions.Callstack;
            TraceSource.Listeners["console"].TraceOutputOptions |= TraceOptions.LogicalOperationStack;
        }
    }
}