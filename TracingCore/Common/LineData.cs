using Microsoft.CodeAnalysis;

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

    public class TraceLineSpan : ILineData
    {
        public int StartLine { private set; get; }
        public int StartCharacter { private set; get; }
        public int EndLine { private set; get; }
        public int EndCharacter { private set; get; }
        public bool UseEndLine { get; }
        public int TraceLine => UseEndLine ? EndLine : StartLine;

        public TraceLineSpan(FileLinePositionSpan lineSpan, bool useEndLine)
        {
            Assign(lineSpan);
            UseEndLine = useEndLine;
        }

        private void Assign(FileLinePositionSpan lineSpan)
        {
            StartLine = lineSpan.StartLinePosition.Line + 1;
            StartCharacter = lineSpan.StartLinePosition.Character + 1;
            EndLine = lineSpan.EndLinePosition.Line + 1;
            EndCharacter = lineSpan.EndLinePosition.Character + 1;
        }
    }
}