using TracingCore.TraceToPyDtos;

namespace TracingCore.Data
{
    public abstract class OptVariableBase
    {
        public object Value { get; }
        public string Type { get; }
        public bool IsRef { get; }
        public int? HeapId { get; }

        public OptVariableBase(object value, string type, bool isRef, int? heapId)
        {
            Value = value;
            Type = type;
            IsRef = isRef;
            HeapId = heapId;
        }

        public static bool IsVarRef(VariableData variableData)
        {
            return !(variableData.Value == null || variableData.IsValueType || variableData.Value is string);
        }
    }

    public class OptVariableData : OptVariableBase
    {
        public string Name { get; }

        public OptVariableData(string name, object value, string type, bool isRef, int? heapId) :
            base(value, type, isRef, heapId)
        {
            Name = name;
        }

        public static OptVariableData GetReturnValue(VariableData variableData, int? heapId)
        {
            var name = "__return__";
            var isRef = IsVarRef(variableData);
            return new OptVariableData(name, variableData.Value,
                variableData.Value == null ? null : variableData.Type.ToString(), isRef, heapId);
        }
    }
}