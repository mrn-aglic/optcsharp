using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class StatementInstrumentation
    {
        private readonly ExpressionGenerator _expressionGenerator;

        public StatementInstrumentation(ExpressionGenerator expressionGenerator)
        {
            _expressionGenerator = expressionGenerator;
        }

        public InstrumentationData PrepareTraceData(CompilationUnitSyntax root)
        {
            var methods = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
            var blocks = methods.Select(x => x.Body).ToList();

            var statementsData = new List<InstrumentationDetails>();

            foreach (var blockSyntax in blocks)
            {
                var lineNumbers = blockSyntax.Statements.Where(x => !(x is ReturnStatementSyntax)).Select(x => RoslynHelper.GetLineData(x)).Skip(1)
                    .Append(blockSyntax.Statements.Any() ? RoslynHelper.GetLineData(blockSyntax.Statements.Last()) : null);

                foreach (var (statement, lineNum) in blockSyntax.Statements.Zip(lineNumbers))
                {
                    var egDetails = new ExpressionGeneratorDetails.Long
                    (
                        SubtreeType.MethodInvocation,
                        "TraceApi",
                        "TraceData",
                        lineNum,
                        statement,
                        false
                    );
                    var statementToInsert =
                        _expressionGenerator.GetExpressionStatement(egDetails);

                    var statementData = new InstrumentationDetails(statement, statementToInsert, Insert.After);
                    statementsData.Add(statementData);
                }
            }

            return new InstrumentationData(root, statementsData);
        }
    }
}