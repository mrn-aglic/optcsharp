using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;
using SyntaxNode = Microsoft.CodeAnalysis.SyntaxNode;

namespace TracingCore.SourceCodeInstrumentation
{
    public class InstrumentationShared
    {
        private readonly ExpressionGenerator _expressionGenerator;
        private const string TraceApiClass = "TraceApi";
        private const string TraceApiEnterMethod = "TraceMethodEntry";
        private const string TraceApiMethodFirstStep = "TraceData";
        private const string TraceApiExitMethod = "TraceMethodExit";
        private const string TraceApiReturnExitMethod = "TraceMethodReturnExit";

        private readonly string _returnVarTemplate = "__return_{0}";

        public InstrumentationShared(ExpressionGenerator expressionGenerator)
        {
            _expressionGenerator = expressionGenerator;
        }

        private SyntaxNode GetTarget(IEnumerable<StatementSyntax> statements, MethodTrace methodTrace)
        {
            switch (methodTrace)
            {
                case MethodTrace.Entry:
                    return statements.First();
                case MethodTrace.Exit:
                    return statements.Last();
                case MethodTrace.FirstStep:
                    return statements.First();
                default:
                    throw new ArgumentException("Wrong MethodTrace value");
            }
        }

        private InstrumentationDetails InstrumentReturnStatement
        (
            TypeSyntax returnType,
            ReturnStatementSyntax returnStatement
        )
        {
            var lineData = RoslynHelper.GetLineData(returnStatement);
            var variableName = string.Format(_returnVarTemplate, lineData.StartLine);

            var newLocalDeclarationStatement =
                _expressionGenerator.FromReturnToLocalDeclaration(variableName, returnType, returnStatement);
            var rewrittenReturnStatement = _expressionGenerator.ReturnVariableStatement(variableName);

            var generatorDetails = new ExpressionGeneratorDetails.Long(
                TraceApiClass,
                TraceApiReturnExitMethod,
                lineData,
                newLocalDeclarationStatement,
                false
            );

            var traceStatement = _expressionGenerator.GetExpressionStatement(generatorDetails);

            var block = _expressionGenerator.WrapInBlock(
                newLocalDeclarationStatement,
                traceStatement,
                rewrittenReturnStatement
            );

            return new InstrumentationDetails(
                returnStatement,
                block,
                Insert.Replace
            );
        }

        public IEnumerable<InstrumentationDetails> InstrumentReturnStatements
        (
            MethodDeclarationSyntax declarationSyntax,
            IEnumerable<ReturnStatementSyntax> returnStatements
        )
        {
            var returnType = declarationSyntax.ReturnType;
            return returnStatements.Select(x => InstrumentReturnStatement(returnType, x));
        }
        
        public IEnumerable<InstrumentationDetails> InstrumentReturnStatements
        (
            TypeSyntax returnType,
            IEnumerable<ReturnStatementSyntax> returnStatements
        )
        {
            return returnStatements.Select(x => InstrumentReturnStatement(returnType, x));
        }

        internal InstrumentationDetails GetBodyInsStatement
        (
            BlockSyntax body,
            bool hasStatements,
            bool includeThisReference,
            MethodTrace methodTrace
        )
        {
            var declarationSyntax = body.Parent;
            var statements = body.Statements;

            var target = hasStatements ? GetTarget(statements, methodTrace) : body;
            var targetLine = methodTrace == MethodTrace.Entry
                ? RoslynHelper.GetLineData(declarationSyntax)
                : RoslynHelper.GetLineData(target, !hasStatements);

            var traceMethodName = methodTrace == MethodTrace.Entry
                ? TraceApiEnterMethod
                : methodTrace == MethodTrace.Exit
                    ? TraceApiExitMethod
                    : TraceApiMethodFirstStep;

            var egDetails = new ExpressionGeneratorDetails.Long(
                TraceApiClass,
                traceMethodName,
                targetLine,
                declarationSyntax,
                includeThisReference
            );

            var statementToInsert = methodTrace == MethodTrace.Entry
                ? _expressionGenerator.GetExpressionStatement(egDetails)
                : methodTrace == MethodTrace.FirstStep
                    ? _expressionGenerator.GetDullExpressionStatement(egDetails)
                    : _expressionGenerator.GetExitExpressionStatement(egDetails);

            var insertWhere = methodTrace == MethodTrace.Entry || methodTrace == MethodTrace.FirstStep
                ? Insert.Before
                : Insert.After;
            return new InstrumentationDetails(target, statementToInsert.NormalizeWhitespace(), insertWhere);
        }

        public InstrumentationDetails GetMethodInsStatementDetails
        (
            BaseMethodDeclarationSyntax declarationSyntax,
            bool hasStatements,
            bool includeThisReference,
            MethodTrace methodTrace
        )
        {
            return GetBodyInsStatement(declarationSyntax.Body, hasStatements, includeThisReference, methodTrace);
        }

        public InstrumentationData GetMethodInsData
        (
            BaseMethodDeclarationSyntax declarationSyntax,
            CompilationUnitSyntax root
        )
        {
            var includeThisReference = !declarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

            var body = declarationSyntax.Body;
            var hasStatements = body.Statements.Any();

            var enterDetails = GetMethodInsStatementDetails(declarationSyntax, hasStatements, includeThisReference,
                MethodTrace.Entry);
            var dullDetails = GetMethodInsStatementDetails(declarationSyntax, hasStatements, includeThisReference,
                MethodTrace.FirstStep);
            var exitDetails =
                GetMethodInsStatementDetails(declarationSyntax, hasStatements, includeThisReference, MethodTrace.Exit);

            var listOfDetails = new List<InstrumentationDetails>();

            if (hasStatements)
            {
                listOfDetails.Add(enterDetails);
                listOfDetails.Add(dullDetails);
                listOfDetails.Add(exitDetails);

                return new InstrumentationData(root, listOfDetails);
            }

            var newBody = body.AddStatements(
                (ExpressionStatementSyntax) enterDetails.StatementToInsert,
                (ExpressionStatementSyntax) dullDetails.StatementToInsert,
                (ExpressionStatementSyntax) exitDetails.StatementToInsert);

            listOfDetails.Add(new InstrumentationDetails(declarationSyntax.Body,
                newBody,
                Insert.Replace
            ));

            return new InstrumentationData(root, listOfDetails);
        }
    }
}