namespace TracingCore.Common
{
    public interface ILineData
    {
        public int StartLine { get; }
    }
    public class LineData : ILineData
    {
        public int StartLine { get; }

        public LineData(int startLine)
        {
            StartLine = startLine;
        }
    }

    public class FullLineData : ILineData
    {
        public int StartLine { get; }
        public int Column { get; }

        public FullLineData(int startLine, int startLineColumn)
        {
            StartLine = startLine;
            Column = startLineColumn;
        }
    }
}