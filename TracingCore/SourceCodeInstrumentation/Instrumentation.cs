using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        public Instrumentation(ExpressionGenerator expressionGenerator)
        {
            var eg = expressionGenerator;
            _statementInstrumentation = new StatementInstrumentation(eg);
            _methodInstrumentation = new MethodInstrumentation(new InstrumentationShared(eg));
            _classInstrumentation = new ClassInstrumentation(new InstrumentationShared(eg), eg);
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            // ROOT NEEDS TO BE MUTATED 
            // THE LAST ROOT MUTATED WILL BE THE FIRST PASTED TO INJECTION CODE.
            var insData = _statementInstrumentation.PrepareTraceData(root);
            var classInsData = _classInstrumentation.PrepareTraceData(root);
            var methodInsData = _methodInstrumentation.PrepareTraceData(root);
            var staticConsData = _classInstrumentation.PrepareStaticConstructor(root);

            var newRoot = InjectTraceApi(methodInsData);
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, classInsData.Statements));
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, insData.Statements));
            newRoot = InjectTraceApi(new InstrumentationData(newRoot, staticConsData.Statements));
            return newRoot;
        }

        public CompilationUnitSyntax InjectTraceApi(InstrumentationData data)
        {
            var root = data.Root;
            var statements = data.Statements.Select(x => x.InsStatement).ToList();
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
                    case Insert.InsertAttribute:
                        var method = currentStatement as MethodDeclarationSyntax;
                        root = root.ReplaceNode(
                            method,
                            new[]
                            {
                                method.WithAttributeLists(
                                    method.AttributeLists.Add((AttributeListSyntax) statementToInsert))
                            }
                        );
                        break;
                    case Insert.Member:
                        var currentClass = currentStatement as ClassDeclarationSyntax;
                        var constructor = statementToInsert as ConstructorDeclarationSyntax;
                        var hasMembers = currentClass.Members.Any();
                        if (hasMembers)
                        {
                            root = root.InsertNodesAfter(currentClass.Members.Last(), new[] {constructor});
                        }
                        else
                        {
                            root = root.ReplaceNode(currentClass, currentClass
                                .WithMembers(new SyntaxList<MemberDeclarationSyntax>().Add(constructor)));
                        }

                        break;
                }
            }

            return root.NormalizeWhitespace();
        }

        public void WriteToFile(CompilationUnitSyntax root)
        {
            FileIO.WriteToFile(root.ToString(), "Instrumentation", "code.txt");
        }
    }
}