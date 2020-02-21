using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;

namespace TracingCore.RoslynRewriters
{
    public class SourceCodeRewriter : CSharpSyntaxRewriter, IInstrumentationEngine
    {
        private readonly SyntaxAnnotation _locationAnnotation;
        private readonly ExpressionGenerator _expressionGenerator;
        private readonly PropertyInstrumentationConfig _propertyConfig;
        private readonly string _returnVarTemplate;

        private const string AnnotationKind = "location";


        private readonly HashSet<SyntaxKind> _methodLikeDeclarations = new HashSet<SyntaxKind>
        {
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration
        };

        public SourceCodeRewriter(ExpressionGenerator expressionGenerator, InstrumentationConfig instrumentationConfig)
        {
            _expressionGenerator = expressionGenerator;
            _propertyConfig = instrumentationConfig.Property;
            _returnVarTemplate = instrumentationConfig.ReturnVarTemplate;

            _locationAnnotation = new SyntaxAnnotation(AnnotationKind);
        }

        private ConstructorDeclarationSyntax PrepareStaticConstructor(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var lineData = RoslynHelper.GetLineData(classDeclarationSyntax);
            var egDetails = new ExpressionGeneratorDetails.Short(
                TraceApiNames.ClassName,
                TraceApiNames.RegisterClassLoad,
                lineData
            );

            var staticCotr = _expressionGenerator.CreateStaticConstructorForClass(classDeclarationSyntax);
            var fullyQualifiedName = RoslynHelper.GetClassParentPath(classDeclarationSyntax);
            var registerStatement =
                _expressionGenerator.GetRegisterClassLoadExpression(egDetails, staticCotr, fullyQualifiedName);
            return staticCotr.WithBody(_expressionGenerator.WrapInBlock(registerStatement));
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
                ? PrepareStaticConstructor(node)
                : base.Visit(implStaticCotr));

            var backupProperties = node.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(x =>
                    x.WithIdentifier(
                        SyntaxFactory.Identifier($"{_propertyConfig.BackupNamePrefix}{x.Identifier.Text}")
                    )
                );

            var visitsForEachMember = node.Members.Select(x => base.Visit(x) as MemberDeclarationSyntax);
            var members = new SyntaxList<MemberDeclarationSyntax>().AddRange(visitsForEachMember)
                .AddRange(backupProperties);
            var nodeWithNewMembers = node.WithMembers(members);

            return staticCotrMember != null ? nodeWithNewMembers.AddMembers(staticCotrMember) : nodeWithNewMembers;
        }

        private ExpressionGeneratorDetails.Long GetEgDetails(SyntaxNode node, string traceApiMethod,
            bool includeThisReference, bool useEndLine = false)
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                traceApiMethod,
                RoslynHelper.GetLineData(node, useEndLine),
                node,
                includeThisReference
            );
            return egDetails;
        }

        private LineData[] GetLineNumbers(List<StatementSyntax> statements)
        {
            var lineNumbers = statements.Skip(1).Select(x => RoslynHelper.GetLineData(x));
            var lastStatement = statements.LastOrDefault();
            var anyStatements = lastStatement != null;

            var lineNums = anyStatements
                ? lineNumbers.Append(RoslynHelper.GetLineData(lastStatement)).ToList()
                : lineNumbers;

            return lineNums.ToArray();
        }

        private bool IsNodeMethodLike(SyntaxNode node)
        {
            return _methodLikeDeclarations.Contains(node.Kind());
        }

        private bool NodeHasStaticModifier(SyntaxNode node)
        {
            return node switch
            {
                AccessorDeclarationSyntax accessorDeclarationSyntax => accessorDeclarationSyntax.Modifiers.Any(x =>
                    x.IsKind(SyntaxKind.StaticKeyword)),
                BaseMethodDeclarationSyntax baseMethodDeclarationSyntax => baseMethodDeclarationSyntax.Modifiers.Any(
                    x => x.IsKind(SyntaxKind.StaticKeyword)),
                _ => throw new NotImplementedException(
                    "The use case probably is not implemented yet or a bug occurred.")
            };
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var parent = node.Parent;
            var statements = node.Statements.ToList();
            var hasStatements = statements.Any();
            var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().First();

            var isImmediateParentMethodLike = IsNodeMethodLike(parent);

            var includeThisReference = isImmediateParentMethodLike &&
                                       classDeclaration.Modifiers.All(x => !x.IsKind(SyntaxKind.StaticKeyword)) &&
                                       !NodeHasStaticModifier(parent);

            var entryName = isImmediateParentMethodLike
                ? TraceApiNames.TraceMethodEntry
                : TraceApiNames.TraceBlockEntry;
            var exitName = isImmediateParentMethodLike ? TraceApiNames.TraceMethodExit : TraceApiNames.TraceBlockExit;

            var dullTarget = hasStatements ? statements.First() : node;
            var exitTarget = hasStatements ? statements.Last() : node;

            var egDetails = GetEgDetails(parent, entryName, includeThisReference);
            var dullDetails = GetEgDetails(dullTarget, TraceApiNames.TraceApiMethodFirstStep, includeThisReference,
                !hasStatements);
            var exitDetails = GetEgDetails(exitTarget, exitName, includeThisReference,
                !hasStatements);

            var enterMethodStatement = _expressionGenerator.GetExpressionStatement(egDetails);
            var dullMethodStatement = _expressionGenerator.GetDullExpressionStatement(dullDetails);

            var blockSpecificStatements = new List<StatementSyntax>
                {enterMethodStatement, dullMethodStatement};

            var insStatements = statements.Zip(GetLineNumbers(statements))
                .SelectMany(x => InstrumentStatement(x.First, x.Second)).ToList();

            blockSpecificStatements.AddRange(insStatements);

            if (insStatements.Any() && insStatements.Last().IsKind(SyntaxKind.ReturnStatement))
                return node.WithStatements(new SyntaxList<StatementSyntax>().AddRange(blockSpecificStatements));

            var exitMethodStatement =
                _expressionGenerator.GetExitExpressionStatement(exitDetails, hasStatements,
                    isImmediateParentMethodLike);
            blockSpecificStatements.Add(exitMethodStatement);

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
                _ => InstrumentSimpleStatement(statement, lineNum)
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

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            var hasBlock = node.Statement.IsKind(SyntaxKind.Block);
            if (hasBlock) return node.WithStatement((StatementSyntax) VisitBlock(node.Statement as BlockSyntax));

            var root = node.SyntaxTree.GetRoot();

            var ifWithBlock = node.WithStatement(_expressionGenerator.WrapInBlock(node.Statement))
                .WithAdditionalAnnotations(_locationAnnotation);

            root = root.ReplaceNode(node, ifWithBlock);
            var @if = (IfStatementSyntax) root.GetAnnotatedNodes(_locationAnnotation).First();

            return @if.WithStatement((StatementSyntax) VisitBlock(@if.Statement as BlockSyntax));
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
            var returnLineData = RoslynHelper.GetLineData(statement);
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

        private StatementSyntax GetSimpleTraceStatement(StatementSyntax statement, LineData lineNum)
        {
            var egDetails = new ExpressionGeneratorDetails.Long
            (
                TraceApiNames.ClassName,
                TraceApiNames.TraceData,
                lineNum,
                statement,
                false
            );
            return _expressionGenerator.GetExpressionStatement(egDetails);
        }

        private List<StatementSyntax> InstrumentSimpleStatement(StatementSyntax statement, LineData lineNum)
        {
            var statementToInsert = GetSimpleTraceStatement(statement, lineNum);

            return new List<StatementSyntax> {statement, statementToInsert};
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            return node.WithBody((BlockSyntax) VisitBlock(node.Body));
        }

        public CompilationUnitSyntax Start(CompilationUnitSyntax root)
        {
            var syntaxTree = base.Visit(root).SyntaxTree;
            return syntaxTree.GetCompilationUnitRoot().NormalizeWhitespace();
        }
    }
}