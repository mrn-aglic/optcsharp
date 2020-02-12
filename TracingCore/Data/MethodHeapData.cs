namespace TracingCore.Data
{
    public class MethodHeapData : HeapData
    {
        public override HeapType HeapType => HeapType.Function;
        public string FuncName { get; }
        public string Declaration { get; }
        public string EnclosingParentFullName { get; }

        public MethodHeapData(int heapId, string funcName, string enclosingParentFullName, string declaration) : base(heapId, null)
        {
            FuncName = funcName;
            Declaration = declaration;
            EnclosingParentFullName = enclosingParentFullName;
        }

        public MethodHeapData Copy()
        {
            return new MethodHeapData(HeapId, FuncName, EnclosingParentFullName, Declaration);
        }
    }
}