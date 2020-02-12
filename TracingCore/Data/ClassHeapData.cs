namespace TracingCore.Data
{
    public class ClassHeapData : HeapData
    {
        public override HeapType HeapType => HeapType.Class;
        public string FullyQualifiedName { get; }
        public string[] Extends { get; }

        public ClassHeapData(int heapId, string fullyQualifiedName, string[] extends) : base(heapId, null)
        {
            FullyQualifiedName = fullyQualifiedName;
            Extends = extends;
        }
    }
}