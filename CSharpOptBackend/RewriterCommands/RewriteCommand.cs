using System.Collections.Generic;
using CSharpOptBackend.DebuggerCommands;
using CSharpOptBackend.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;

namespace CSharpOptBackend.RewriterCommands
{
    public class RewriteCommand : ICommand
    {
        public int InsertAt { get; }
        public DebugStatement Statement { get; }

        public RewriteCommand(int insertAt, DebugStatement statement)
        {
            InsertAt = insertAt;
            Statement = statement;
        }

        public static RewriteCommand GetCommand(StatementSyntax node, IEnumerable<string> variables)
        {
            var debugCommand = new DebugStatement(node, variables);
            return new RewriteCommand(node.GetSpan().FirstLine.Begin, debugCommand);
        }
    }
}