using RoslynExtensions.Models;

namespace RoslynExtensions.interfaces
{
    public interface INodeSpan
    {
        public Span GetSingleLineHighlight();
        public Span GetMultilineHighlight();
    }
}