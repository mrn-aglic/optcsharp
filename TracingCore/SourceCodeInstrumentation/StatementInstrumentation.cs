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
            var accessors = root.DescendantNodes().OfType<AccessorDeclarationSyntax>();
            var blocks = methods.Select(x => x.Body).Concat(accessors.Select(x => x.Body)).ToList();

            var statementsData = new List<InstrumentationDetails>();

            foreach (var blockSyntax in blocks)
            {
                var data = InstrumentBlock(blockSyntax);
                statementsData.AddRange(data);
            }

            return new InstrumentationData(root, statementsData);
        }

        private InstrumentationDetails PrepareStatementDetails(StatementSyntax statementSyntax, LineData lineNum)
        {
            switch (statementSyntax)
            {
                case IfStatementSyntax ifStatementSyntax:
                    return InstrumentIfStatement(ifStatementSyntax, lineNum);
                case ThrowStatementSyntax throwStatementSyntax:
                    return InstrumentThrowStatement(throwStatementSyntax, lineNum);
                default:
                    return InstrumentSimpleStatement(statementSyntax, lineNum);
            }
        }

        private InstrumentationDetails InstrumentThrowStatement(ThrowStatementSyntax throwStatementSyntax,
            LineData lineNum)
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                lineNum,
                throwStatementSyntax,
                false
            );

            var statementToInsert =
                _expressionGenerator.GetExpressionStatement(egDetails);

            return new InstrumentationDetails(throwStatementSyntax, statementToInsert, Insert.Before);
        }

        private List<InstrumentationDetails> InstrumentBlock(BlockSyntax blockSyntax)
        {
            var nonReturnStatements = blockSyntax.Statements.Where(x => !(x is ReturnStatementSyntax || x is IfStatementSyntax)).ToList();
            var lineNumbers = nonReturnStatements
                .Select(x => RoslynHelper.GetLineData(x))
                .Skip(1)
                .ToList();
            var lineNums = (nonReturnStatements.Any()
                ? lineNumbers.Append(RoslynHelper.GetLineData(nonReturnStatements.Last()))
                : lineNumbers).ToList();

            var statementsWithNums = nonReturnStatements.Zip(lineNums);

            return InstrumentNonReturnStatements(statementsWithNums);
        }

        private List<InstrumentationDetails> InstrumentNonReturnStatements(
            IEnumerable<(StatementSyntax, LineData)> statementsWithNums)
        {
            return statementsWithNums.Select(x => PrepareStatementDetails(x.Item1, x.Item2)).ToList();
        }

        private InstrumentationDetails InstrumentIfStatement
        (
            IfStatementSyntax ifStatementSyntax,
            LineData lineNum
        )
        {
            var hasBody = ifStatementSyntax.Statement is BlockSyntax;
            var body = ifStatementSyntax.Statement as BlockSyntax;

            var statement = ifStatementSyntax.Statement;
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                lineNum,
                ifStatementSyntax,
                false
            );
            var statementToInsertForIf = _expressionGenerator.GetExpressionStatement(egDetails);

            if (hasBody)
                return new InstrumentationDetails(body.Statements.First(), statementToInsertForIf, Insert.Before);

            var instrumentationDetails = InstrumentNonReturnStatements(new List<(StatementSyntax, LineData)>
                {(statement, RoslynHelper.GetLineData(statement))}).First();
            var block = instrumentationDetails.Insert == Insert.Before
                ? _expressionGenerator.WrapInBlock(statementToInsertForIf,
                    instrumentationDetails.StatementToInsert as StatementSyntax,
                    statement)
                : _expressionGenerator.WrapInBlock(
                    statementToInsertForIf, statement, instrumentationDetails.StatementToInsert as StatementSyntax);

            return
                new InstrumentationDetails(statement, block, Insert.Replace);
        }

        private InstrumentationDetails InstrumentSimpleStatement(StatementSyntax statement, LineData lineNum)
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                lineNum,
                statement,
                false
            );
            var statementToInsert =
                _expressionGenerator.GetExpressionStatement(egDetails);

            return new InstrumentationDetails(statement, statementToInsert, Insert.After);
        }
    }
}