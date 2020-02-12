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

    public class BlockLineData : LineData, ILineData
    {
        public BlockLineData(int startLine, int endLine) : base(startLine)
        {
        }
    }
}