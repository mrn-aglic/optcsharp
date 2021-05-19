using System.Collections.Generic;
using CSharpOptBackend.DebuggerCommands;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpOptBackend.Interfaces
{
    public interface IDebugStatementManager : ICommandManager
    {
        public IList<DebugStatement> CreateDebugStatements(CompilationUnitSyntax compilationUnit);
    }
}