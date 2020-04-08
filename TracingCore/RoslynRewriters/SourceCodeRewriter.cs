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

namespace TracingCore.RoslynRewriters
{
    public class SourceCodeRewriter : CSharpSyntaxRewriter, IInstrumentationEngine
    {
        private readonly bool _extractForDeclaration = false;

        private readonly ExpressionGenerator _expressionGenerator;
        private readonly PropertyInstrumentationConfig _propertyConfig;
        private readonly string _returnVarTemplate;

        private readonly AnnotationsManager _annotationsManager;

        private readonly HashSet<SyntaxKind> _methodLikeDeclarations = new HashSet<SyntaxKind>
        {
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration
        };

        private readonly HashSet<string> _structs;

        public SourceCodeRewriter(ExpressionGenerator expressionGenerator, InstrumentationConfig instrumentationConfig)
        {
            _expressionGenerator = expressionGenerator;
            _propertyConfig = instrumentationConfig.Property;
            _returnVarTemplate = instrumentationConfig.ReturnVarTemplate;

            _annotationsManager = new AnnotationsManager();
            _structs = new HashSet<string>();
        }

        private ConstructorDeclarationSyntax PrepareStaticConstructor(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var lineData = GetLineData(classDeclarationSyntax);
            var egDetails = new ExpressionGeneratorDetails.Short(
                TraceApiNames.ClassName,
                TraceApiNames.RegisterClassLoad,
                lineData
            );

            var staticCotr = _expressionGenerator.CreateStaticConstructorForClass(classDeclarationSyntax);
            var fullyQualifiedName = GetClassParentPath(classDeclarationSyntax);
            var registerStatement =
                _expressionGenerator.GetRegisterClassLoadExpression(egDetails, staticCotr, fullyQualifiedName);
            return staticCotr.WithBody(_expressionGenerator.WrapInBlock(registerStatement));
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var backupProperties = node.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                    x.WithIdentifier(
                            SyntaxFactory.Identifier($"{_propertyConfig.BackupNamePrefix}{x.Identifier.Text}")
                        )
                        .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation)
                );

            var visitsForEachMember = node.Members.Select(x => base.Visit(x) as MemberDeclarationSyntax);
            var members = new SyntaxList<MemberDeclarationSyntax>().AddRange(visitsForEachMember)
                .AddRange(backupProperties);
            var nodeWithNewMembers = node.WithMembers(members);

            return nodeWithNewMembers.WithAdditionalAnnotations(_annotationsManager.OriginalLine(node));
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var implStaticCotr = node.Members.FirstOrDefault(x =>
                x.IsKind(SyntaxKind.ConstructorDeclaration) &&
                x.Modifiers.Any(SyntaxKind.StaticKeyword)
            );
            var hasStaticConstructor = implStaticCotr != null;

            var hasMain = node.Members.OfType<MethodDeclarationSyntax>()
                .Any(x => x.Identifier.Text == "Main" &&
                          x.Modifiers.Any(SyntaxKind.StaticKeyword));
            var staticCotrMember = (MemberDeclarationSyntax) (!hasStaticConstructor &&
                                                              !hasMain
                ? PrepareStaticConstructor(node).WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation)
                : base.Visit(implStaticCotr));

            var backupProperties = node.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                    x.WithIdentifier(
                            SyntaxFactory.Identifier($"{_propertyConfig.BackupNamePrefix}{x.Identifier.Text}")
                        )
                        .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation)
                );

            var visitsForEachMember = node.Members.Select(x => base.Visit(x) as MemberDeclarationSyntax);
            var members = new SyntaxList<MemberDeclarationSyntax>().AddRange(visitsForEachMember)
                .AddRange(backupProperties);
            var nodeWithNewMembers = node.WithMembers(members);

            var newNode = staticCotrMember != null
                ? nodeWithNewMembers.AddMembers(staticCotrMember)
                : nodeWithNewMembers;

            return newNode.WithAdditionalAnnotations(_annotationsManager.OriginalLine(node));
        }

        private ExpressionGeneratorDetails.Long GetEgDetails
        (
            SyntaxNode node,
            LineData lineData,
            string traceApiMethod,
            bool includeThisReference
        )
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                traceApiMethod,
                lineData,
                node,
                includeThisReference
            );
            return egDetails;
        }

        private bool NodeHasStaticModifier(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax classDeclarationSyntax =>
                classDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)),
                AccessorDeclarationSyntax accessorDeclarationSyntax => accessorDeclarationSyntax.Modifiers.Any(x =>
                    x.IsKind(SyntaxKind.StaticKeyword)),
                BaseMethodDeclarationSyntax baseMethodDeclarationSyntax => baseMethodDeclarationSyntax.Modifiers.Any(
                    x => x.IsKind(SyntaxKind.StaticKeyword)),
                _ => throw new NotImplementedException(
                    "The use case probably is not implemented yet or a bug occurred.")
            };
        }

        private LineData[] GetLineNumbers(BlockSyntax blockSyntax, bool isParentMethodLike, bool isAugmentation)
        {
            var statements = blockSyntax.Statements;

            if (isAugmentation && statements.Any()) return new[] {GetLineData(blockSyntax.Statements.Last())};

            var prevAndNext = statements.Zip(statements.Skip(1), (p, c) => (Previous: p, Current: c));
            var lineNumbers = prevAndNext.Select(x => GetLineData(x.Current))
                .Append(isParentMethodLike && statements.Any()
                    ? GetLineData(statements.Last(), true)
                    : GetLineData(blockSyntax, true));
            return lineNumbers.ToArray();
        }

        private bool IsNodeMethodLike(SyntaxNode node)
        {
            return _methodLikeDeclarations.Contains(node.Kind());
        }
        
        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var isAugmentation = _annotationsManager.IsAugmentation(node);
            var parent = node.Parent;
            var statements = node.Statements.ToList();
            var hasStatements = statements.Any();

            var @class = node.Ancestors().OfType<ClassDeclarationSyntax>().First();
            var isParentMethodLike = IsNodeMethodLike(parent);

            var includeThisReference = isParentMethodLike &&
                                       @class.Modifiers.All(x => !x.IsKind(SyntaxKind.StaticKeyword)) &&
                                       !NodeHasStaticModifier(parent);

            var entryName = isParentMethodLike
                ? TraceApiNames.TraceMethodEntry
                : TraceApiNames.TraceBlockEntry;
            var exitName = isParentMethodLike
                ? TraceApiNames.TraceMethodExit
                : TraceApiNames.TraceBlockExit;

            var lineNumbers = GetLineNumbers(node, isParentMethodLike, isAugmentation);
            var insStatements = statements.Zip(lineNumbers)
                .SelectMany(x => InstrumentStatement(x.First, x.Second)).ToList();

            var entryLineData = IsNodeMethodLike(parent)
                ? GetLineData(parent)
                : GetLineData(node);
            var enterDetails = GetEgDetails(parent, entryLineData, entryName, includeThisReference);

            var dullLineNumber = hasStatements
                ? GetLineData(statements.First())
                : GetLineData(node, true);

            var dullDetails = GetEgDetails(statements.FirstOrDefault(), dullLineNumber,
                TraceApiNames.TraceData, includeThisReference);

            var enterBlockStatement = _expressionGenerator.GetExpressionStatement(enterDetails);
            var dullStatement = _expressionGenerator.GetDullExpressionStatement(dullDetails);

            var specialStatements = new List<StatementSyntax>();
            specialStatements.Add(enterBlockStatement);

            if (!isAugmentation)
            {
                specialStatements.Add(dullStatement);
            }

            var blockSpecificStatements = specialStatements.Concat(insStatements).ToList();

            if (insStatements.Any() && insStatements.Last().IsKind(SyntaxKind.ReturnStatement)) // TODO will need to fix
                return node.WithStatements(new SyntaxList<StatementSyntax>().AddRange(blockSpecificStatements));

            var exitLineNumber = hasStatements ? lineNumbers.Last() : GetLineData(node, true);
            var exitDetails = GetEgDetails(statements.LastOrDefault(), exitLineNumber, exitName, includeThisReference);
            var exitBlockStatement =
                _expressionGenerator.GetExitExpressionStatement(exitDetails, hasStatements,
                    isParentMethodLike);

            blockSpecificStatements.Add(exitBlockStatement);

            return node.WithStatements(new SyntaxList<StatementSyntax>().AddRange(blockSpecificStatements));
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return node.WithBody(VisitBlock(node.Body) as BlockSyntax);
        }

        private List<StatementSyntax> InstrumentStatement(StatementSyntax statement, LineData lineNum)
        {
            return statement switch
            {
                ReturnStatementSyntax returnStatementSyntax => InstrumentReturnStatement(returnStatementSyntax),
                ThrowStatementSyntax throwStatementSyntax => InstrumentThrowStatement(throwStatementSyntax, lineNum),
                IfStatementSyntax ifStatementSyntax => new List<StatementSyntax>
                {
                    VisitIfStatement(ifStatementSyntax) as StatementSyntax,
                    GetSimpleTraceStatement(statement, lineNum)
                },
                ForStatementSyntax forStatementSyntax => HandleForLoop(forStatementSyntax, lineNum),
                WhileStatementSyntax whileStatementSyntax => new List<StatementSyntax>
                {
                    VisitWhileStatement(whileStatementSyntax) as StatementSyntax,
                    GetSimpleTraceStatement(whileStatementSyntax, lineNum)
                },
                DoStatementSyntax doStatementSyntax => new List<StatementSyntax>
                {
                    VisitDoStatement(doStatementSyntax) as StatementSyntax,
                    GetSimpleTraceStatement(doStatementSyntax, lineNum)
                },
                ForEachStatementSyntax forEachStatementSyntax => new List<StatementSyntax>
                {
                    VisitForEachStatement(forEachStatementSyntax) as StatementSyntax
                },
                _ => InstrumentSimpleStatement(statement, lineNum)
            };
        }

        private List<StatementSyntax> HandleForLoop(ForStatementSyntax forStatementSyntax, LineData lineNum)
        {
            return new List<StatementSyntax>
            {
                VisitForStatement(forStatementSyntax) as StatementSyntax,
                GetSimpleTraceStatement(forStatementSyntax, lineNum, !_extractForDeclaration)
            };
        }

        private List<StatementSyntax> InstrumentThrowStatement(ThrowStatementSyntax throwStatementSyntax,
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

            return new List<StatementSyntax> {statementToInsert, throwStatementSyntax};
        }

        private BlockSyntax WrapInBlock(StatementSyntax node)
        {
            var root = node.SyntaxTree.GetRoot();
            var parent = node.Parent;

            var dummyBlock = _expressionGenerator.WrapInBlock(node)
                .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation);

            SyntaxNode typedParent = parent switch
            {
                IfStatementSyntax ifStatementSyntax => ifStatementSyntax.WithStatement(dummyBlock),
                ElseClauseSyntax elseClauseSyntax => elseClauseSyntax.WithStatement(dummyBlock),
                _ => throw new NotImplementedException()
            };
            var nodeWithAnnotation = typedParent
                .WithAdditionalAnnotations(_annotationsManager.LocationAnnotation);

            root = root.ReplaceNode(parent, nodeWithAnnotation);
            var result = root.GetAnnotatedNodes(_annotationsManager.LocationAnnotation).First();
            return result.ChildNodes().OfType<BlockSyntax>().First();
        }

        private ElseClauseSyntax HandleElseClause(ElseClauseSyntax @else)
        {
            if (@else == null) return null;

            var statement = @else.Statement;
            var hasBlock = statement.IsKind(SyntaxKind.Block);

            return hasBlock
                ? @else.WithStatement((StatementSyntax) VisitBlock(@else.Statement as BlockSyntax))
                : @else.WithStatement(VisitBlock(WrapInBlock(@else.Statement)) as BlockSyntax);
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);

            var elseClause = HandleElseClause(node.Else);

            if (hasBlock)
                return node
                    .WithStatement((StatementSyntax) VisitBlock(node.Statement as BlockSyntax))
                    .WithElse(elseClause);

            return node
                .WithStatement(VisitBlock(WrapInBlock(node.Statement)) as BlockSyntax)
                .WithElse(elseClause);
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);

            var block = hasBlock
                ? VisitBlock(node.Statement as BlockSyntax) as BlockSyntax
                : WrapInBlock(node.Statement);

            // var forKeywordTrace = GetSimpleTraceStatement(node, GetLineData(node));
            var blockWithLoopTrace =
                block.WithStatements(block.Statements
                    .InsertRange(0, new[] {_expressionGenerator.GetRegisterLoopIteration(node)}));

            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.ConditionalTrace,
                GetLineData(node),
                node,
                false,
                false
            );

            return node.WithStatement(blockWithLoopTrace)
                .WithIncrementors(
                    new SeparatedSyntaxList<ExpressionSyntax>()
                        .AddRange(node.Incrementors)
                        .Add(
                            _expressionGenerator.StatementToExpressionSyntax(
                                _expressionGenerator.GetConditionalExpressionStatement(egDetails, node.Condition)
                            )
                        )
                );
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);

            var block = hasBlock
                ? VisitBlock(node.Statement as BlockSyntax) as BlockSyntax
                : WrapInBlock(node.Statement);

            var blockWithLoopTrace =
                block.WithStatements(block.Statements
                    .InsertRange(0, new[] {_expressionGenerator.GetRegisterLoopIteration(node)}));

            return node.WithStatement(blockWithLoopTrace);
        }

        public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);

            var block = hasBlock
                ? VisitBlock(node.Statement as BlockSyntax) as BlockSyntax
                : WrapInBlock(node.Statement);

            var blockWithLoopTrace = block.WithStatements(block.Statements
                .InsertRange(0, new[] {_expressionGenerator.GetRegisterLoopIteration(node)}));

            // TODO implement
            return node.WithStatement(blockWithLoopTrace);
        }

        private List<StatementSyntax> InstrumentReturnStatement
        (
            TypeSyntax returnType,
            LineData lineData,
            ReturnStatementSyntax returnStatement
        )
        {
            var variableName = string.Format(_returnVarTemplate, lineData.StartLine);

            var newLocalDeclarationStatement =
                _expressionGenerator.FromReturnToLocalDeclaration(variableName, returnType, returnStatement);
            var rewrittenReturnStatement = _expressionGenerator.ReturnVariableStatement(variableName);

            var generatorDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceMethodReturnExit,
                lineData,
                newLocalDeclarationStatement,
                false
            );

            var traceStatement = _expressionGenerator.GetExpressionStatement(generatorDetails);

            return new List<StatementSyntax>
            {
                newLocalDeclarationStatement,
                traceStatement,
                rewrittenReturnStatement
            };
        }

        private List<StatementSyntax> InstrumentReturnStatement(ReturnStatementSyntax statement)
        {
            var returnLineData = GetLineData(statement);
            var firstBlockSyntax = statement.Ancestors().OfType<BlockSyntax>().First();
            var parentType = firstBlockSyntax.Parent switch
            {
                MethodDeclarationSyntax methodDeclarationSyntax => methodDeclarationSyntax.ReturnType,
                AccessorDeclarationSyntax accessorDeclaration => ((PropertyDeclarationSyntax) accessorDeclaration.Parent
                    .Parent).Type,
                _ => throw new ArgumentException($"{statement.Parent.Kind()} case not implemented.")
            };
            var returnType = parentType;

            return InstrumentReturnStatement(returnType, returnLineData, statement);
        }

        private StatementSyntax GetSimpleTraceStatement(StatementSyntax statement, LineData lineNum,
            bool excludeDeclaration = false)
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                lineNum,
                statement,
                false,
                excludeDeclaration
            );
            return _expressionGenerator.GetExpressionStatement(egDetails);
        }

        private List<StatementSyntax> InstrumentSimpleStatement(StatementSyntax statement, LineData lineNum)
        {
            var statementToInsert = GetSimpleTraceStatement(statement, lineNum);

            var symbol = _expressionGenerator.SemanticModel.GetSymbolInfo(statement);

            return new List<StatementSyntax> {statement, statementToInsert};
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            return node.WithBody((BlockSyntax) VisitBlock(node.Body));
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);

            if (hasBlock)
                return node.WithStatement((StatementSyntax) VisitBlock(node.Statement as BlockSyntax));

            var root = node.SyntaxTree.GetRoot();
            var forEachWithBlock = node.WithStatement(_expressionGenerator.WrapInBlock(node.Statement))
                .WithAdditionalAnnotations(_annotationsManager.LocationAnnotation);

            root = root.ReplaceNode(node, forEachWithBlock);
            var @foreach =
                (ForEachStatementSyntax) root.GetAnnotatedNodes(_annotationsManager.LocationAnnotation).First();

            return @foreach.WithStatement((StatementSyntax) VisitBlock(@foreach.Statement as BlockSyntax));
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            var syntaxTree = base.Visit(root).SyntaxTree;
            return syntaxTree.GetCompilationUnitRoot().NormalizeWhitespace();
        }
    }
}