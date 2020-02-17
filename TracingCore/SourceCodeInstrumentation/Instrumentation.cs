using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class Instrumentation
    {
        private readonly StatementInstrumentation _statementInstrumentation;
        private readonly MethodInstrumentation _methodInstrumentation;
        private readonly ClassInstrumentation _classInstrumentation;
        private readonly PropertyInstrumentation _propertyInstrumentation;

        public Instrumentation(ExpressionGenerator expressionGenerator)
        {
            var eg = expressionGenerator;
            _statementInstrumentation = new StatementInstrumentation(eg);
            _methodInstrumentation = new MethodInstrumentation(new InstrumentationShared(eg));
            _classInstrumentation = new ClassInstrumentation(new InstrumentationShared(eg), eg);
            _propertyInstrumentation = new PropertyInstrumentation(new InstrumentationShared(eg));
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

                switch (insDetails.Insert)
                {
                    case Insert.After:
                        root = root.InsertNodesAfter(currentStatement,
                            new[]
                            {
                                statementToInsert
                            });
                        break;
                    case Insert.Before:
                        root = root.InsertNodesBefore(currentStatement,
                            new[]
                            {
                                statementToInsert
                            });
                        break;
                    case Insert.Replace:
                        if (currentStatement is ReturnStatementSyntax)
                        {
                            root = root.ReplaceNode(currentStatement, statementToInsert.ChildNodes());
                        }
                        else
                        {
                            root = root.ReplaceNode(currentStatement, statementToInsert);
                        }

                        break;
                    case Insert.Member:
                        var currentClass = currentStatement as ClassDeclarationSyntax;
                        var newMember = statementToInsert as MemberDeclarationSyntax;
                        var hasMembers = currentClass.Members.Any();
                        if (hasMembers)
                        {
                            root = root.InsertNodesAfter(currentClass.Members.Last(), new[] {newMember});
                        }
                        else
                        {
                            root = root.ReplaceNode(currentClass, currentClass
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>().Add(newMember)));
                        }

                        break;
                }
            }

            return root.NormalizeWhitespace();
        }

        public static void WriteToFile(CompilationUnitSyntax root)
        {
            FileIO.WriteToFile(root.ToString(), "Instrumentation", "code.txt");
        }
    }
}