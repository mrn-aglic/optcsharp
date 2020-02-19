using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class PropertyInstrumentation
    {
        private readonly InstrumentationShared _instrumentationShared;
        private readonly PropertyInstrumentationConfig _propertyInstrumentationConfig;

        public PropertyInstrumentation(InstrumentationShared instrumentationShared, PropertyInstrumentationConfig propertyInstrumentationConfig)
        {
            _instrumentationShared = instrumentationShared;
            _propertyInstrumentationConfig = propertyInstrumentationConfig;
        }

        public InstrumentationData PrepareTraceData(CompilationUnitSyntax root)
        {
            var insData = new List<InstrumentationData>();
            var proxyPropertiesData = new List<InstrumentationDetails>();

            var declarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
            foreach (var declarationSyntax in declarations)
            {
                var data = PrepareTraceData(declarationSyntax, root);
                insData.Add(data);

                var proxyProperty = PrepareProxyProperty(declarationSyntax);
                proxyPropertiesData.Add(proxyProperty);
            }

            return new InstrumentationData(root, insData.SelectMany(x => x.Statements).Concat(proxyPropertiesData).ToList());
        }

        private InstrumentationDetails PrepareProxyProperty
        (
            PropertyDeclarationSyntax propertyDeclarationSyntax
        )
        {
            var propNamePrefix = _propertyInstrumentationConfig.BackupNamePrefix;
            var newPropertyName = $"{propNamePrefix}{propertyDeclarationSyntax.Identifier.Text}";
            var propProxy = propertyDeclarationSyntax.WithIdentifier(
                SyntaxFactory.Identifier(newPropertyName)
            );

            return new InstrumentationDetails(
                propertyDeclarationSyntax.Parent, propProxy, Insert.Member
            );
        }

        private InstrumentationData PrepareTraceData
        (
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            CompilationUnitSyntax root
        )
        {
            var includeThisReference =
                !propertyDeclarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

            var accessors = propertyDeclarationSyntax.AccessorList.Accessors;
            var propertyType = propertyDeclarationSyntax.Type;

            var accessorDetails =
                accessors.Select(x => InstrumentAccessor(x, propertyType, includeThisReference, root));

            return new InstrumentationData(root, accessorDetails.SelectMany(x => x.Statements).ToList());
        }

        private InstrumentationData InstrumentAccessor(
            AccessorDeclarationSyntax accessorDeclaration,
            TypeSyntax propertyType,
            bool includeThisReference,
            CompilationUnitSyntax root)
        {
            var body = accessorDeclaration.Body;
            var statements = body.Statements;
            var hasStatements = statements.Any();

            var enterDetails = GetAccessorInsStatementDetails(
                accessorDeclaration,
                hasStatements,
                includeThisReference,
                MethodTrace.Entry);

            var dullDetails = GetAccessorInsStatementDetails(
                accessorDeclaration,
                hasStatements,
                includeThisReference,
                MethodTrace.FirstStep
            );

            var returnStatements = statements.OfType<ReturnStatementSyntax>().ToList();

            var exitDetailsList = returnStatements.Any()
                ? _instrumentationShared.InstrumentReturnStatements(propertyType, returnStatements)
                : new List<InstrumentationDetails>
                {
                    GetAccessorInsStatementDetails(
                        accessorDeclaration,
                        hasStatements,
                        includeThisReference,
                        MethodTrace.Exit)
                };

            var listOfDetails = new List<InstrumentationDetails>();

            if (hasStatements)
            {
                listOfDetails.Add(enterDetails);
                listOfDetails.Add(dullDetails);
                listOfDetails.AddRange(exitDetailsList);
            }
            else
            {
                var exitDetails = exitDetailsList.First();
                var newBody = body.AddStatements(
                    (ExpressionStatementSyntax) enterDetails.StatementToInsert,
                    (ExpressionStatementSyntax) dullDetails.StatementToInsert,
                    (ExpressionStatementSyntax) exitDetails.StatementToInsert
                );

                listOfDetails.Add(
                    new InstrumentationDetails(
                        body,
                        newBody,
                        Insert.Replace
                    )
                );
            }

            return new InstrumentationData(root, listOfDetails);
        }

        private InstrumentationDetails GetAccessorInsStatementDetails
        (
            AccessorDeclarationSyntax declaration,
            bool hasStatements,
            bool includeThisReference,
            MethodTrace methodTrace
        )
        {
            return _instrumentationShared.GetBodyInsStatement(declaration.Body, hasStatements, includeThisReference,
                methodTrace);
        }
    }
}