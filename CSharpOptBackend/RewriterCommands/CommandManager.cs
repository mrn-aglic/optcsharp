using System.Collections.Generic;
using CSharpOptBackend.RewriterCommands;

namespace CSharpOptBackend
{
    public class CommandManager
    {
        public List<RewriteCommand> RewriteCommands { get; }

        public CommandManager()
        {
            RewriteCommands = new List<RewriteCommand>();
        }

        public void AddCommand(RewriteCommand command)
        {
            RewriteCommands.Add(command);
        }
    }
}