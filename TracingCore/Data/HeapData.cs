namespace TracingCore.Data
{
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
}