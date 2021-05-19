using System.Collections.Generic;
using CSharpOptBackend.DebuggerCommands;
using CSharpOptBackend.Interfaces;
using CSharpOptBackend.Rewriters;
using CSharpOptBackend.Visitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TracingCore;
using TracingCore.Data;
using TracingCore.Interceptors;

namespace CSharpOptBackend
{
    public class OptBackend
    {
        public string Code { get; }
        public CSharpSyntaxTree UserSyntaxTree { get; }


        public Compiler Compiler { get; }
        public ConsoleHandler ConsoleHandler { get; }
        public ExecutionManager ExecutionManager { get; }
        public PyTutorDataManager PyTutorDataManager { get; }

        public OptBackend(string code)
        {
            UserSyntaxTree = CSharpSyntaxTree.ParseText(code) as CSharpSyntaxTree;
        }

        public IEnumerable<DebugStatement> GetDebuggerCommands(IDebugStatementManager debugStatementManager)
        {
            var compilationUnitRoot = UserSyntaxTree.GetCompilationUnitRoot();
            var visitor = new CSharpWalker(debugStatementManager);
            visitor.Visit(compilationUnitRoot);

            return debugStatementManager.CreateDebugStatements(compilationUnitRoot);
        }

        public SyntaxNode PreprocessTree()
        {
            var compilationUnitRoot = UserSyntaxTree.GetCompilationUnitRoot();

            var preprocessor = new CSharpPreprocess();
            return preprocessor.Visit(compilationUnitRoot);
        }

        // public IEnumerable<INodeDetails> GetNodeDetails(IDetailsManager detailsManager)
        // {
        //     var visitor = new CSharpWalker(detailsManager);
        //     visitor.Visit(UserSyntaxTree.GetRoot());
        //     return detailsManager.GetDetails();
        // }
    }
}