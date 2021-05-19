using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.DebuggerCommands
{
    public class ScopedVar
    {
        public SyntaxNode Scope { get; }
        public string Name { get; }

        public ScopedVar(SyntaxNode scope, string name)
        {
            Scope = scope;
            Name = name;
        }
    }
}