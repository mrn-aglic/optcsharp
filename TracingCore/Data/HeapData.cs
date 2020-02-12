using System.Collections;
using System.Collections.Generic;
using TracingCore.TraceToPyDtos;

namespace TracingCore.Data
{
    public enum HeapType
    {
        Instance,
        Class,
        Function,
        List
    }

    public abstract class HeapData
    {
        public int HeapId { get; }
        public object Value { get; }

        public abstract HeapType HeapType { get; }

        public HeapData(int heapId, object value)
        {
            HeapId = heapId;
            Value = value;
        }
    }

    public class VarHeapData : HeapData
    {
        public string Type { get; }
        public object Value { get; }
        public override HeapType HeapType { get; }

        public VarHeapData(int heapId, string type, object value) : base(heapId, value)
        {
            Type = type;
            Value = value;
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

    public class MethodHeapData : HeapData
    {
        public override HeapType HeapType => HeapType.Function;
        public int HeapId { get; }
        public string FuncName { get; }
        public string Declaration { get; }
        public string EnclosingParentFullName { get; }

        public MethodHeapData(int heapId, string funcName, string enclosingParentFullName, string declaration) : base(heapId, null)
        {
            HeapId = heapId;
            FuncName = funcName;
            Declaration = declaration;
            EnclosingParentFullName = enclosingParentFullName;
        }

        public MethodHeapData Copy()
        {
            return new MethodHeapData(HeapId, FuncName, EnclosingParentFullName, Declaration);
        }
    }

    public class ClassHeapData : HeapData
    {
        public override HeapType HeapType => HeapType.Class;
        public int HeapId { get; }
        public string FullyQualifiedName { get; }
        // public IList<MethodHeapData> Methods { get; }

        public ClassHeapData(int heapId, string fullyQualifiedName) : base(heapId, null)
        {
            HeapId = heapId;
            FullyQualifiedName = fullyQualifiedName;
            // Methods = methods;
        }
    }
}