using System;
using System.Collections.Generic;
using System.Linq;
using CSharpOptBackend.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpOptBackend.DebuggerCommands
{
    public class DebugStatementManager : IDebugStatementManager
    {
        public IList<DebugStatement> DebuggerCommands { get; }

        public DebugStatementManager()
        {
            DebuggerCommands = new List<DebugStatement>();
        }

        public void AddCommand(ICommand command)
        {
            switch (command)
            {
                case DebugStatement cmd:
                    DebuggerCommands.Add(cmd);
                    break;
                default: throw new ArgumentException("Unsupported command for DebuggerCommands.CommandManager");
            }
        }

        public IList<DebugStatement> GetCommands()
        {
            return DebuggerCommands.ToList();
        }

        public DebugStatement CreateCommand(SyntaxNode node)
        {
            throw new NotImplementedException();
        }


        private DebugStatement CollectCommands(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var parameters = methodDeclarationSyntax.ParameterList.Parameters;

            var debuggerCommands =
                new DebugStatement(methodDeclarationSyntax, parameters.Select(p => p.Identifier.Text));

            return debuggerCommands;
        }
    }
}