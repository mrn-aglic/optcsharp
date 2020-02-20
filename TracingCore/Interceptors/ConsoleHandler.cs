using System;
using System.Collections.Generic;
using System.IO;

namespace TracingCore.Interceptors
{
    public class ConsoleHandler
    {
        private ConsoleReader _consoleReader;
        private ConsoleWriter _consoleWriter;

        private TextReader _defaultInputStream;
        private TextWriter _defaultOutputStream;
        
        public ConsoleHandler()
        {
            _consoleWriter = new ConsoleWriter();
            _consoleReader = new ConsoleReader(Console.Out);
            Init();
        }

        public void Init()
        {
            _defaultInputStream = Console.In;
            _defaultOutputStream = Console.Out;
            
            Console.SetIn(_consoleReader);
            Console.SetOut(_consoleWriter);
        }

        public void RestoreDefaults()
        {
            Console.SetIn(_defaultInputStream);
            Console.SetOut(_defaultOutputStream);
        }

        public string GetConsoleOutput()
        {
            return _consoleWriter.GetOutput();
        }

        public void AddToRead(string element)
        {
            _consoleReader.Add(element);
        }
        
        public void AddRangeToRead(IList<string> inputs)
        {
            foreach (string input in inputs)
            {
                AddToRead(input);
            }
        }
    }
}