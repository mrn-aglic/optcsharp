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
                // IfStatementSyntax ifStatementSyntax => 
                // new List<InstrumentationDetails> {InstrumentIfStatement(ifStatementSyntax), InstrumentSimpleStatement(statementSyntax, lineNum)},
                _ => InstrumentSimpleStatement(statementSyntax, lineNum)
            };
        }

        private List<InstrumentationDetails> PrepareNestedStatementsDetails(StatementSyntax statementSyntax,
            LineData lineNum)
        {
            return statementSyntax switch
            {
                IfStatementSyntax ifStatementSyntax => InstrumentIfStatement(ifStatementSyntax),
                _ => new List<InstrumentationDetails>()
            };
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

        private InstrumentationDetails CreateBlockEnterStepDetails
        (
            BlockSyntax blockSyntax,
            LineData lineData,
            bool includeThisReference = false
        )
        {
            var statements = blockSyntax.Statements.ToList();
            var hasStatements = statements.Any();

            return
                _instrumentationShared.GetBodyInsStatement(blockSyntax, hasStatements, includeThisReference,
                    MethodTrace.FirstStep, lineData); //TODO separate method entry from block entry
        }

        private IEnumerable<(StatementSyntax, LineData)> ZipWitLineData(StatementSyntax[] statements,
            StatementSyntax lastStatement)
        {
            if (lastStatement == null) return new List<(StatementSyntax, LineData)>();

            var lineNumbers = statements.Skip(1).Select(x => RoslynHelper.GetLineData(x));
            var anyStatements = statements.Any();
            var lineNums = anyStatements
                ? lineNumbers.Append(RoslynHelper.GetLineData(lastStatement)).ToList()
                : lineNumbers;

            return statements.Zip(lineNums);
        }

        private List<InstrumentationDetails> BlockDetails(BlockSyntax blockSyntax, bool includeThisReference = false)
        {
            var list = new List<InstrumentationDetails>();
            var statements = blockSyntax.Statements;
            var nonReturnStatements = blockSyntax.Statements.Where(x => !x.IsKind(SyntaxKind.ReturnStatement));
            var statementsWithLineData = ZipWitLineData(nonReturnStatements.ToArray(), statements.LastOrDefault());
            var statementsDetails = statementsWithLineData.SelectMany(x =>
                new List<InstrumentationDetails> {PrepareStatementDetails(x.Item1, x.Item2)}
                    .Concat(PrepareNestedStatementsDetails(x.Item1, x.Item2))
            );

            if (!(blockSyntax.Parent is BaseMethodDeclarationSyntax || blockSyntax.Parent is AccessorDeclarationSyntax))
            {
                var enterStepDetails = CreateBlockEnterStepDetails(blockSyntax, includeThisReference);
                list.Add(enterStepDetails);
            }

            list.AddRange(statementsDetails);

            return list;
        }

        private InstrumentationDetails SingleStatementBlockDetails
        (
            StatementSyntax statement,
            bool includeThisReference = false
        )
        {
            var enterLine = RoslynHelper.GetLineData(statement);
            var exitLine = enterLine;

            var egEnterDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                enterLine,
                statement.Parent,
                includeThisReference
            );

            var egExitDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                exitLine,
                statement,
                includeThisReference
            );

            var enterStatement = _expressionGenerator.GetExpressionStatement(egEnterDetails);
            var statementDetails = PrepareStatementDetails(statement, exitLine);
            var beforeExitStatement = statementDetails.StatementToInsert as StatementSyntax;
            var block = statementDetails.Insert switch
            {
                Insert.After => _expressionGenerator.WrapInBlock(enterStatement, statement, beforeExitStatement),
                Insert.Before => _expressionGenerator.WrapInBlock(enterStatement, beforeExitStatement, statement),
            };

            return new InstrumentationDetails(statement, block, Insert.Replace);
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

        private List<InstrumentationDetails> InstrumentIfStatement
        (
            IfStatementSyntax ifStatementSyntax
        )
        {
            var hasBody = ifStatementSyntax.Statement.IsKind(SyntaxKind.Block);
            var body = ifStatementSyntax.Statement as BlockSyntax;

            if (hasBody) return BlockDetails(body);

            var statement = ifStatementSyntax.Statement;
            var details = SingleStatementBlockDetails(statement);

            return new List<InstrumentationDetails>
            {
                details
            };
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