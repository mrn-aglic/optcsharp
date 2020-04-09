using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.SyntaxTreeEnhancers;
using TracingCore.TreeRewriters;
using static TracingCore.Common.RoslynHelper;
using static TracingCore.Common.Declarations;

namespace TracingCore.RoslynRewriters
{
    public class SourceRewriter : CSharpSyntaxRewriter, IInstrumentationEngine
    {
        private readonly bool _extractForDeclaration = false;

        private readonly RewriteHelper _rewriteHelper;

        private readonly ExpressionGenerator _expressionGenerator;
        private readonly PropertyInstrumentationConfig _propertyConfig;
        private readonly string _returnVarTemplate;

        private readonly AnnotationsManager _annotationsManager;

        public SourceRewriter(ExpressionGenerator expressionGenerator, InstrumentationConfig instrumentationConfig)
        {
            _expressionGenerator = expressionGenerator;
            _propertyConfig = instrumentationConfig.Property;
            _returnVarTemplate = instrumentationConfig.ReturnVarTemplate;

            _annotationsManager = new AnnotationsManager();

            _rewriteHelper = new RewriteHelper(expressionGenerator);
        }

        private StatementSyntax GetNextStatement(BlockSyntax blockSyntax, StatementSyntax currentStatement)
        {
            if (blockSyntax == null) return null;
            var index = blockSyntax.Statements.IndexOf(currentStatement);
            return index == blockSyntax.Statements.Count - 1 ? null : blockSyntax.Statements[index + 1];
        }

        private LineData GetLineNumber(StatementSyntax node, BlockSyntax block)
        {
            if (block == null)
            {
                return GetLineData(node, true);
            }

            var parentIsMethod = IsMethodLike(block.Parent);
            var nextStatement = GetNextStatement(block, node);
            return
                nextStatement != null ? GetLineData(nextStatement) :
                parentIsMethod ? GetLineData(block.Statements.Last(), true) :
                GetLineData(block, true);
        }

        private SyntaxNode ReplaceSingleLineStatementWithBlock(SyntaxNode node, SyntaxNode source, BlockSyntax block)
        {
            return node switch
            {
                IfStatementSyntax ifStatement => ifStatement.WithStatement(
                    GetBlockForStatement(ifStatement, block, () => GetLineData(source), () => GetLineData(source))
                ),
                ElseClauseSyntax elseClause => elseClause.WithStatement(
                    GetBlockForStatement(elseClause, block, () => GetLineData(source), () => GetLineData(source))
                ),
                _ => throw new NotImplementedException()
            };
        }

        private SyntaxNode InstrumentBlock
        (
            BlockSyntax blockSyntax,
            SyntaxNode parent,
            ClassDeclarationSyntax @class,
            IReadOnlyCollection<StatementSyntax> newStatements
        )
        {
            var statements = blockSyntax.Statements;
            var hasStatements = statements.Any();
            var isParentMethodLike = IsMethodLike(parent);

            var entryLine = isParentMethodLike
                ? GetLineData(blockSyntax.Parent)
                : GetLineData(blockSyntax);
            var dullLine = hasStatements
                ? GetLineData(statements.First())
                : GetLineData(blockSyntax, true);
            var exitLine = isParentMethodLike && hasStatements
                ? GetLineData(statements.Last(), true)
                : GetLineData(blockSyntax, true);

            var includeThisReference = isParentMethodLike &&
                                       @class.Modifiers.All(x => !x.IsKind(SyntaxKind.StaticKeyword)) &&
                                       !NodeHasStaticModifier(parent);
            
            var entryStatement = isParentMethodLike
                ? _expressionGenerator.GetMethodEntryExpression(entryLine, blockSyntax)
                : _expressionGenerator.GetBlockEntryExpression(entryLine, blockSyntax);

            var dullStatement = _expressionGenerator.GetDullExpressionStatement(dullLine);

            var statementsList = new List<StatementSyntax> {entryStatement, dullStatement}.Concat(newStatements);
            var stmtsList = newStatements.Any() && newStatements.Last().IsKind(SyntaxKind.ReturnStatement)
                ? statementsList
                : statementsList.Append(
                    isParentMethodLike
                        ? _expressionGenerator.GetMethodExitExpression(exitLine,
                            blockSyntax)
                        : _expressionGenerator.GetBlockExitExpression(exitLine, blockSyntax)
                );

            return blockSyntax.WithStatements(new SyntaxList<StatementSyntax>().AddRange(stmtsList));
        }

        private SyntaxNode InstrumentBlock(BlockSyntax blockSyntax, SyntaxNode parent,
            List<StatementSyntax> newStatements)
        {
            var @class = blockSyntax.Ancestors().OfType<ClassDeclarationSyntax>().First();
            return InstrumentBlock(blockSyntax, parent, @class, newStatements);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var statements = node.Statements;
            var newStatements = statements.SelectMany(InstrumentStatement);

            var instrumentedBlock = InstrumentBlock(node, node.Parent, newStatements.ToList());
            return instrumentedBlock;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            return node == null
                ? base.Visit(null)
                : base.Visit(node).WithAdditionalAnnotations(_annotationsManager.OriginalLineAnnotation);
        }

        private List<StatementSyntax> InstrumentStatement(StatementSyntax statement)
        {
            return _rewriteHelper.UnwrapAugmentedBlock(base.Visit(statement));
        }

        private SyntaxNode GetSimpleStatementWithTrace(StatementSyntax statementSyntax, LineData lineData,
            bool excludeDeclaration)
        {
            var newStatement =
                _expressionGenerator.GetSimpleTraceExpression(lineData, statementSyntax, excludeDeclaration);
            return _rewriteHelper.WrapInBlock(statementSyntax, newStatement);
        }

        private SyntaxNode GetSimpleStatementWithTrace(StatementSyntax statementSyntax, bool excludeDeclaration)
        {
            var lineData = GetLineNumber(statementSyntax, statementSyntax.Parent as BlockSyntax);
            return GetSimpleStatementWithTrace(statementSyntax, lineData, excludeDeclaration);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return GetSimpleStatementWithTrace(node, false);
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            return GetSimpleStatementWithTrace(node, false);
        }

        private BlockSyntax GetBlockForStatement
        (
            SyntaxNode node,
            BlockSyntax blockSyntax,
            Func<LineData> entryLine,
            Func<LineData> exitLine
        )
        {
            var enterBlockStatement = _expressionGenerator.GetBlockEntryExpression(entryLine(), node);
            var statements = new List<StatementSyntax> {enterBlockStatement}
                .Concat(blockSyntax.Statements).ToArray();

            var stmts = _annotationsManager.IsAugmentation(blockSyntax)
                ? statements
                : statements.Append(_expressionGenerator.GetBlockExitExpression(exitLine(), node)
                );
            return _expressionGenerator.WrapInBlock(stmts.ToArray());
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            var hasBlock = node.Statement is BlockSyntax;
            var ifNode = (IfStatementSyntax) base.VisitIfStatement(node);

            var @if = hasBlock
                ? ifNode
                : ReplaceSingleLineStatementWithBlock(ifNode,
                        node.Statement,
                        ifNode.Statement is BlockSyntax blockSyntax
                            ? blockSyntax
                            : _rewriteHelper.WrapInBlock(ifNode.Statement)
                    )
                    as IfStatementSyntax;

            if (!node.Parent.IsKind(SyntaxKind.Block)) return @if;

            var lineData = GetLineData(node, true);
            var traceStatement = _expressionGenerator.GetSimpleTraceExpression(lineData, @if, false);
            return _rewriteHelper.WrapInBlock(@if, traceStatement);
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            var hasBlock = node.Statement is BlockSyntax;
            var elseClause = (ElseClauseSyntax) base.VisitElseClause(node);

            if (hasBlock || node.Statement is IfStatementSyntax) return elseClause;

            return ReplaceSingleLineStatementWithBlock(elseClause, node.Statement, elseClause.Statement as BlockSyntax);
        }
        
        private List<StatementSyntax> InstrumentReturnStatement
        (
            TypeSyntax returnType,
            LineData lineData,
            ReturnStatementSyntax returnStatement,
            BlockSyntax containingBlock
        )
        {
            // TODO replace with returnStatement.GetLocation.StartLine_StartColumn
            var variableName = string.Format(_returnVarTemplate, lineData.StartLine);

            var newLocalDeclarationStatement =
                _expressionGenerator.FromReturnToLocalDeclaration(variableName, returnType, returnStatement);
            var rewrittenReturnStatement = _expressionGenerator.ReturnVariableStatement(variableName);

            var traceStatement =
                _expressionGenerator.GetMethodExitExpression(lineData, newLocalDeclarationStatement, containingBlock);

            return new List<StatementSyntax>
            {
                newLocalDeclarationStatement,
                traceStatement,
                rewrittenReturnStatement
            };
        }

        public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
        {
            var returnLineData = GetLineData(node);
            var firstDeclaration = node.Ancestors().OfType<MemberDeclarationSyntax>().First();

            var parentType = firstDeclaration switch
            {
                MethodDeclarationSyntax methodDeclaration => methodDeclaration.ReturnType,
                PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type,
                _ => throw new ArgumentException($"{node.Parent.Kind()} case not implemented.")
            };
            var returnType = parentType;

            var hasBlock = node.Parent is BlockSyntax;
            var block = (BlockSyntax) (hasBlock ? node.Parent : _rewriteHelper.WrapInBlock(node));

            return _rewriteHelper
                .WrapInBlock(InstrumentReturnStatement(returnType, returnLineData, node, block)
                    .ToArray());
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var declaration = (ClassDeclarationSyntax) base.VisitClassDeclaration(node);

            var implStaticCotr = declaration.Members.FirstOrDefault(x =>
                x.IsKind(SyntaxKind.ConstructorDeclaration) &&
                x.Modifiers.Any(SyntaxKind.StaticKeyword)
            );
            var hasStaticConstructor = implStaticCotr != null;

            var hasMain = declaration.Members.OfType<MethodDeclarationSyntax>()
                .Any(x => x.Identifier.Text == "Main" &&
                          x.Modifiers.Any(SyntaxKind.StaticKeyword));

            var staticCotrMember = (MemberDeclarationSyntax) (!hasStaticConstructor &&
                                                              !hasMain
                ? _rewriteHelper.PrepareStaticConstructor(node)
                    .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation)
                : null);

            var backupProperties = node.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                    x.WithIdentifier(
                            SyntaxFactory.Identifier($"{_propertyConfig.BackupNamePrefix}{x.Identifier.Text}")
                        )
                        .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation)
                );

            var nodeWithNewMembers =
                declaration.AddMembers(backupProperties.Select(x => (MemberDeclarationSyntax) x).ToArray());

            var fullDeclaration = staticCotrMember != null
                ? nodeWithNewMembers.AddMembers(staticCotrMember)
                : nodeWithNewMembers;

            return fullDeclaration.WithAdditionalAnnotations(_annotationsManager.OriginalLine(node));
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            var syntaxTree = base.Visit(root).SyntaxTree;
            return syntaxTree.GetCompilationUnitRoot().NormalizeWhitespace();
        }
    }
}