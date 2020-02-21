namespace TracingCore.TreeRewriters
{
    public interface ILineData
    {
    }
    public class LineData : ILineData
    {
        public int StartLine { get; }
        public LineData(int startLine)
        {
            StartLine = startLine;
        }
    }
}