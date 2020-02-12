using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore
{
    public class SyntaxElementsRepository
    {
        public Dictionary<int, StatementSyntax> Statements { get; }

        public SyntaxElementsRepository()
        {
            
        }
        
        public void Register(int statementLine, StatementSyntax statementSyntax)
        {
            
        }
    }
}