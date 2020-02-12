using System.Collections;
using TracingCore.TraceToPyDtos;

namespace TracingCore.Data
{
    public class VarHeapData : HeapData
    {
        public string Type { get; }
        public override HeapType HeapType { get; }

        public VarHeapData(int heapId, string type, object value) : base(heapId, value)
        {
            Type = type;
            HeapType = DetermineType(value);
        }

        public HeapType DetermineType(object value)
        {
            switch (value)
            {
                case IEnumerable _:
                    return HeapType.List;
                case ClassData _:
                    return HeapType.Class;
                case MethodData _:
                    return HeapType.Function;
                default:
                    return HeapType.Instance;
            }
        }

        public VarHeapData Copy(object newValue)
        {
            return new VarHeapData(HeapId, Type, newValue);
        }
    }
}