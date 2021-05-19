using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpOptBackend.TreeDetails
{
    public class Span
    {
        public int LineStart { get; }
        public int LineEnd { get; }
        public int ColumnStart { get; }
        public int ColumnEnd { get; }

        public Span(int lineStart, int lineEnd, int columnStart, int columnEnd)
        {
            LineStart = lineStart;
            LineEnd = lineEnd;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;
        }

        public Span(FileLinePositionSpan span) : this(
            span.StartLinePosition.Line + 1,
            span.EndLinePosition.Line + 1,
            span.StartLinePosition.Character + 1,
            span.EndLinePosition.Character + 1)
        {
        }

        public Span(Location location) : this(location.GetLineSpan())
        {
        }

        public override string ToString()
        {
            return $"Line: {LineStart}-{LineEnd}\nColumn: {ColumnStart}-{ColumnEnd}";
        }
    }
}