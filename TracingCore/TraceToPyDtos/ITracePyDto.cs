using TracingCore.Data;

namespace TracingCore.TraceToPyDtos
{
    public interface ITracePyDto
    {
        HeapData ToHeapData();
    }
}