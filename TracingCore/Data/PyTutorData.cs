using System.Collections.Generic;

namespace TracingCore.Data
{
    public class PyTutorData
    {
        public string Code { get; }
        public IList<IPyTutorStep> Trace { get; }

        public PyTutorData(string code, IList<IPyTutorStep> trace)
        {
            Code = code;
            Trace = trace;
        }
    }
}