using SyntaxComposer.MessagePassing.interfaces;

namespace SyntaxComposer
{
    public class ComposerInstance : ISyntaxComposer
    {
        public IMethodWrappers MethodWrappers { get; }

        public ComposerInstance(IMethodWrappers methodWrappers)
        {
            MethodWrappers = methodWrappers;
        }
    }
}