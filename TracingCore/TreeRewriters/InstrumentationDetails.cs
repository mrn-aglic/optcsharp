using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TracingCore.TreeRewriters
{
    public class InstrumentationData
    {
        // REMOVE ROOT IN FUTURE
        public CompilationUnitSyntax Root { get; }
        public IList<InstrumentationDetails> Statements { get; }

        public InstrumentationData(CompilationUnitSyntax root, IList<InstrumentationDetails> statements)
        {
            Root = root;
            Statements = statements;
        }
    }

    public enum Insert
    {
        After,
        Before,
        Replace,
        InsertAttribute,
        Member
    }
    public class InstrumentationDetails
    {
        public SyntaxNode InsStatement { get; }
        public SyntaxNode StatementToInsert { get; }
        public Insert Insert { get; }

        public InstrumentationDetails
        (
            SyntaxNode insStatement,
            SyntaxNode statementToInsert,
            Insert insert
        )
        {
            InsStatement = insStatement;
            StatementToInsert = statementToInsert;
            Insert = insert;
        }
    }
}