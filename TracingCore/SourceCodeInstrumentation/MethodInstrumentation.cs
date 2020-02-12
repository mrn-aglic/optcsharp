using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class MethodInstrumentation
    {
        private readonly InstrumentationShared _instrumentationShared;

        public MethodInstrumentation(InstrumentationShared instrumentationShared)
        {
            _instrumentationShared = instrumentationShared;
        }

        public InstrumentationData PrepareTraceData(CompilationUnitSyntax root)
        {
            var insData = new List<InstrumentationData>();
            var declarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            foreach (var declarationSyntax in declarations)
            {
                var data = PrepareTraceData(declarationSyntax, root);
                insData.Add(data);
            }

            return new InstrumentationData(root, insData.SelectMany(x => x.Statements).ToList());
        }

        public InstrumentationData PrepareTraceData
        (
            MethodDeclarationSyntax methodDeclarationSyntax,
            CompilationUnitSyntax root
        )
        {
            var listOfDetails = new List<InstrumentationDetails>();
            var includeThisReference = !methodDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

            var body = methodDeclarationSyntax.Body;
            var statements = body.Statements;
            var hasStatements = body.Statements.Any();

            var enterDetails = _instrumentationShared.GetMethodInsStatement(
                methodDeclarationSyntax,
                hasStatements,
                includeThisReference,
                InstrumentationShared.MethodTrace.Entry
            );
            var dullDetails = _instrumentationShared.GetMethodInsStatement(
                methodDeclarationSyntax,
                hasStatements,
                includeThisReference,
                InstrumentationShared.MethodTrace.FirstStep
            );

            listOfDetails.Add(enterDetails);
            listOfDetails.Add(dullDetails);

            var returnStatements = statements
                .Where(x => x.IsKind(SyntaxKind.ReturnStatement))
                .Select(x => x as ReturnStatementSyntax)
                .ToList();

            var exitDetailsList = returnStatements.Any()
                ? _instrumentationShared.InstrumentReturnStatements(
                    methodDeclarationSyntax,
                    returnStatements
                ).ToList()
                : new List<InstrumentationDetails>
                {
                    _instrumentationShared.GetMethodInsStatement(
                        methodDeclarationSyntax,
                        hasStatements,
                        includeThisReference,
                        InstrumentationShared.MethodTrace.Exit
                    )
                };

            listOfDetails.AddRange(exitDetailsList);

            if (!hasStatements)
            {
                var exitDetails = exitDetailsList.First();
                var newBody = body.AddStatements(
                    (ExpressionStatementSyntax) enterDetails.StatementToInsert,
                    (ExpressionStatementSyntax) dullDetails.StatementToInsert,
                    (ExpressionStatementSyntax) exitDetails.StatementToInsert
                );

                listOfDetails.Add(
                    new InstrumentationDetails(
                        methodDeclarationSyntax.Body,
                        newBody,
                        Insert.Replace
                    )
                );
            }

            var instrumentationData =
                new InstrumentationData(root, listOfDetails);
            return instrumentationData;
        }
    }
}