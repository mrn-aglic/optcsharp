using System.Linq;
using Microsoft.CodeAnalysis;

namespace SyntaxComposer.Extensions
{
    public static class NodeExtensions
    {
        public static T GetParentOfType<T>(this SyntaxNode node)
        {
            return node.Ancestors().OfType<T>().FirstOrDefault();
        }
    }
}