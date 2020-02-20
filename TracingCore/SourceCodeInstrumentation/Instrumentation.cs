using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class Instrumentation : IInstrumentationEngine
    {
        private readonly StatementInstrumentation _statementInstrumentation;
        private readonly MethodInstrumentation _methodInstrumentation;
        private readonly ClassInstrumentation _classInstrumentation;
        private readonly PropertyInstrumentation _propertyInstrumentation;

        public Instrumentation(ExpressionGenerator expressionGenerator, InstrumentationConfig instrumentationConfig)
        {
            var eg = expressionGenerator;
            _statementInstrumentation = new StatementInstrumentation(new InstrumentationShared(eg), eg);
            _methodInstrumentation = new MethodInstrumentation(new InstrumentationShared(eg));
            _classInstrumentation = new ClassInstrumentation(new InstrumentationShared(eg), eg);
            _propertyInstrumentation = new PropertyInstrumentation(new InstrumentationShared(eg), instrumentationConfig.Property);
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            // ROOT NEEDS TO BE MUTATED 
            // THE LAST ROOT MUTATED WILL BE THE FIRST PASTED TO INJECTION CODE.
            var insData = _statementInstrumentation.PrepareTraceData(root);
            var classInsData = _classInstrumentation.PrepareTraceData(root);
            var methodInsData = _methodInstrumentation.PrepareTraceData(root);
            var propertyInsData = _propertyInstrumentation.PrepareTraceData(root);
            var staticConsData = _classInstrumentation.PrepareStaticConstructor(root);

            var newRoot = InjectTraceApi(propertyInsData);
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, methodInsData.Statements));
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, classInsData.Statements));
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, insData.Statements));
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, staticConsData.Statements));

            return newRoot;
        }

        public CompilationUnitSyntax InjectTraceApi(InstrumentationData data)
        {
            var root = data.Root;
            var statements = data.Statements.Select(x => x.InsTarget).ToList();
            var insDetailsList = data.Statements;

            root = root.TrackNodes(root.DescendantNodes());

            foreach (var (statement, insDetails) in statements.Zip(insDetailsList))
            {
                var currentStatement = root.GetCurrentNode(statement);
                var statementToInsert = insDetails.StatementToInsert;

                root = insDetails.Insert switch
                {
                    Insert.After => root.InsertNodesAfter(currentStatement, new[] {statementToInsert}),
                    Insert.Before => root.InsertNodesBefore(currentStatement, new[] {statementToInsert}),
                    Insert.Replace => (currentStatement is ReturnStatementSyntax
                        ? root.ReplaceNode(currentStatement, statementToInsert.ChildNodes())
                        : root.ReplaceNode(currentStatement, statementToInsert)),
                    Insert.Member => HandleInsertMember(currentStatement, statementToInsert, root)
                        .NormalizeWhitespace(),
                    _ => root
                };
            }

            return root.NormalizeWhitespace();
        }

        private CompilationUnitSyntax HandleInsertMember(SyntaxNode currentStatement, SyntaxNode statementToInsert,
            CompilationUnitSyntax root)
        {
            var currentClass = currentStatement as ClassDeclarationSyntax;
            var newMember = statementToInsert as MemberDeclarationSyntax;
            var hasMembers = currentClass.Members.Any();
            if (hasMembers)
            {
                return root.InsertNodesAfter(currentClass.Members.Last(), new[] {newMember});
            }

            return root.ReplaceNode(currentClass, currentClass
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>().Add(newMember)));
        }

        public static void WriteToFile(CompilationUnitSyntax root)
        {
            FileIO.WriteToFile(root.ToString(), "Instrumentation", "code.txt");
        }
    }
}