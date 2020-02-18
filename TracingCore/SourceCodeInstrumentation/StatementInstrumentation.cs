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
        private readonly InstrumentationShared _instrumentationShared;
        private readonly ExpressionGenerator _expressionGenerator;

        public StatementInstrumentation(InstrumentationShared instrumentationShared,
            ExpressionGenerator expressionGenerator)
        {
            _instrumentationShared = instrumentationShared;
            _expressionGenerator = expressionGenerator;
        }

        public InstrumentationData PrepareTraceData(CompilationUnitSyntax root)
        {
            var methods = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>();
            var accessors = root.DescendantNodes().OfType<AccessorDeclarationSyntax>();
            var blocks = methods.Select(x => x.Body).Concat(accessors.Select(x => x.Body)).ToList();

            var statementsData = new List<InstrumentationDetails>();

            foreach (var data in blocks.Select(blockSyntax => BlockDetails(blockSyntax)))
            {
                statementsData.AddRange(data);
            }

            return new InstrumentationData(root, statementsData);
        }

        private InstrumentationDetails PrepareStatementDetails(StatementSyntax statementSyntax, LineData lineNum)
        {
            return statementSyntax switch
            {
                ThrowStatementSyntax throwStatementSyntax => InstrumentThrowStatement(throwStatementSyntax, lineNum),
                // IfStatementSyntax ifStatementSyntax => InstrumentIfStatement(ifStatementSyntax, lineNum),
                // IfStatementSyntax ifStatementSyntax => InstrumentIfStatementNew(ifStatementSyntax, lineNum),
                _ => InstrumentSimpleStatement(statementSyntax, lineNum)
            };
        }

        private InstrumentationDetails InstrumentIfStatementNew(IfStatementSyntax ifStatementSyntax, LineData lineNum)
        {
            return null;
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

        private InstrumentationDetails CreateBlockEnterStepDetails
        (
            BlockSyntax blockSyntax,
            bool includeThisReference = false
        )
        {
            var statements = blockSyntax.Statements.ToList();
            var hasStatements = statements.Any();
            return
                _instrumentationShared.GetBodyInsStatement(blockSyntax, hasStatements, includeThisReference,
                    MethodTrace.FirstStep); //TODO separate method entry from block entry
        }

        private IEnumerable<(StatementSyntax, LineData)> ZipWitLineData(StatementSyntax[] statements)
        {
            var lineNumbers = statements.Skip(1).Select(x => RoslynHelper.GetLineData(x));
            var anyStatements = statements.Any();
            var lineNums = anyStatements
                ? lineNumbers.Append(RoslynHelper.GetLineData(statements.Last())).ToList()
                : lineNumbers;

            return statements.Zip(lineNums);
        }

        private List<InstrumentationDetails> BlockDetails(BlockSyntax blockSyntax, bool includeThisReference = false)
        {
            var list = new List<InstrumentationDetails>();
            var enterStepDetails = CreateBlockEnterStepDetails(blockSyntax, includeThisReference);
            var nonReturnStatements = blockSyntax.Statements.Where(x => !x.IsKind(SyntaxKind.ReturnStatement));
            var statementsWithLineData = ZipWitLineData(nonReturnStatements.ToArray());
            var statementsDetails = statementsWithLineData.Select(x => PrepareStatementDetails(x.Item1, x.Item2));

            list.Add(enterStepDetails);
            list.AddRange(statementsDetails);

            return list;
        }

        private IEnumerable<InstrumentationDetails> InstrumentBlock(BlockSyntax blockSyntax)
        {
            var statements = blockSyntax.Statements;
            var nonReturnStatements = blockSyntax.Statements.Where(x => !(x is ReturnStatementSyntax)).ToList();
            var lineNumbers = nonReturnStatements
                .Select(x => RoslynHelper.GetLineData(x))
                .Skip(1)
                .ToList();
            var lineNums = (statements.Any()
                ? lineNumbers.Append(RoslynHelper.GetLineData(statements.Last()))
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