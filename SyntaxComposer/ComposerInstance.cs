using SyntaxComposer.MessagePassing.interfaces;

namespace SyntaxComposer
{
    public class SyntaxComposer : ISyntaxComposer
    {
        public IMethodWrappers MethodWrappers { get; }

        public SyntaxComposer(IMethodWrappers methodWrappers)
        {
            MethodWrappers = methodWrappers;
        }
    }

    public interface ISyntaxComposer
    {
    }
}