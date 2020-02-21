using System;
using System.Collections.Generic;
using System.IO;

namespace TracingCore.Interceptors
{
    public class ExitExecutionException : Exception
    {
        public override string Message => "Exit execution to prompt user for ReadLine input";
    }

    public class ConsoleReader : TextReader
    {
        private readonly TextWriter _defaultOutStream;
        private readonly Queue<string> _bufferedInputs;

        public bool IsEmpty => _bufferedInputs.Count == 0;

        public ConsoleReader(TextWriter defaultOutStream)
        {
            _defaultOutStream = defaultOutStream;
            _bufferedInputs = new Queue<string>();
        }

        public int Add(string msg)
        {
            _bufferedInputs.Enqueue(msg);
            return _bufferedInputs.Count;
        }

        public override string ReadLine()
        {
            if (!IsEmpty) return _bufferedInputs.Dequeue();
            Console.SetOut(_defaultOutStream);
            throw new ExitExecutionException();
        }
    }
}