using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TracingCore.Interceptors
{
    public class ConsoleWriter : TextWriter
    {
        private readonly IList<string> _msgQueue;
        
        public override Encoding Encoding { get; }

        public ConsoleWriter()
        {
            _msgQueue = new List<string>();
        }

        public override void WriteLine(string value)
        {
            _msgQueue.Add($"{value}\n");
        }

        public override void Write(string value)
        {
            _msgQueue.Add(value);
        }

        public string GetOutput()
        {
            return _msgQueue.Aggregate("", (x, y) => $"{x}{y}");
        }
    }
}