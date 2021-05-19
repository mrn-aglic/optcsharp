using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.DebuggerCommands
{
    public class DebugStack
    {
        public IList<ScopedVar> ScopedVars { get; }

        public DebugStack(IList<ScopedVar> scopedVars)
        {
            ScopedVars = scopedVars;
        }
    }
    
    public class ScopedVar
    {
        public SyntaxNode Scope { get; }
        public string Name { get; }
        public SyntaxNode Statement { get; }

        public ScopedVar(SyntaxNode scope, string name, SyntaxNode statement)
        {
            Scope = scope;
            Name = name;
            Statement = statement;
        }
    }

    public class VariablesScope
    {
        public SyntaxNode Scope { get; }
        public IEnumerable<string> Variables { get; }
        
        public VariablesScope(SyntaxNode scope, IEnumerable<string> variables)
        {
            Scope = scope;
            Variables = variables;
        }
    }
}