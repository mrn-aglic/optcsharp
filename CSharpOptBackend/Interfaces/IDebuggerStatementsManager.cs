using System.Collections.Generic;
using CSharpOptBackend.DebuggerCommands;
using Microsoft.CodeAnalysis;

namespace CSharpOptBackend.Interfaces
{
    public interface IDebuggerCommandManager : ICommandManager
    {
        public IList<DebuggerCommand> GetCommands();
        public DebuggerCommand CreateCommand(SyntaxNode node);
    }
}