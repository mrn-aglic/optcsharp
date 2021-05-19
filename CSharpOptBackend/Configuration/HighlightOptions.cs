using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpOptBackend.Configuration
{
    public class HighlightOptions
    {
        private readonly HashSet<SyntaxKind> _highlightOptionsEnums;

        public HighlightOptions(HashSet<SyntaxKind> highlightOptionsEnums)
        {
            _highlightOptionsEnums = highlightOptionsEnums;
        }

        public bool IsEnabled(SyntaxKind syntaxKind)
        {
            return _highlightOptionsEnums.Contains(syntaxKind);
        }
    }
}