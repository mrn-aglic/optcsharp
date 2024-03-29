using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public class MethodData : ITracePyDto
    {
        public HeapType OptHeapType = HeapType.Function;
        public string Name { get; }
        public int Line { get; }
        public string Declaration { get; }
        public bool IsMain { get; }

        public MethodData(string name, int line, string declaration, bool isMain)
        {
            Name = name;
            Line = line;
            Declaration = declaration;
            IsMain = isMain;
        }

        public HeapData ToHeapData()
        {
            throw new System.NotImplementedException();
        }
    }
}