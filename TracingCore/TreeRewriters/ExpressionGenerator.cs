using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.TraceToPyDtos;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TracingCore.TreeRewriters
{
    public class ExpressionGenerator
    {
        public readonly SemanticModel _semanticModel;

        public ExpressionGenerator(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        private MemberAccessExpressionSyntax GetMemberAccessExpressionSyntax(ExpressionGeneratorDetails details)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(details.ClassName),
                IdentifierName(details.MemberName)
            );
        }

        private MemberAccessExpressionSyntax GetMemberAccessExpressionSyntaxNew(ExpressionGeneratorDetails details)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(details.ClassName),
                    IdentifierName(details.InstanceName)),
                IdentifierName(details.MemberName)
            );
        }

        private IEnumerable<ArgumentSyntax> GetParameters(SyntaxNode syntaxNode, bool excludeDeclaration)
        {
            switch (syntaxNode)
            {
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return methodDeclarationSyntax.ParameterList.Parameters
                        .Select(x => Argument(
                            VariableData.GetObjectCreationSyntax(x)
                        ));
                case ConstructorDeclarationSyntax constructorDeclarationSyntax:
                    return constructorDeclarationSyntax.ParameterList.Parameters
                        .Select(x => Argument(
                            VariableData.GetObjectCreationSyntax(x)
                        ));
                case LocalDeclarationStatementSyntax localDeclarationStatementSyntax:
                    return localDeclarationStatementSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>()
                        .SelectMany(x => x.Variables.Select(y => Argument(
                                VariableData.GetObjectCreationSyntax(y)
                            )
                        ));
                case IfStatementSyntax ifStatementSyntax:
                    var condition = ifStatementSyntax.Condition;
                    var identifiers = condition
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .GroupBy(x => x.Identifier.Text)
                        .Select(group => group.First());
                    return identifiers.Select(x => Argument(VariableData.GetObjectCreationSyntax(x)));
                case ForStatementSyntax forStatementSyntax:
                    var conditionIdentifiers =
                        forStatementSyntax.Condition.DescendantNodes().OfType<IdentifierNameSyntax>();
                    var incrementors =
                        forStatementSyntax.Incrementors.SelectMany(x =>
                            x.DescendantNodes().OfType<IdentifierNameSyntax>());

                    var declaration = forStatementSyntax.Declaration == null
                        ? new HashSet<string>()
                        : forStatementSyntax.Declaration.Variables.Select(x => x.Identifier.Text)
                            .ToHashSet();

                    var allVars = conditionIdentifiers
                        .Concat(incrementors)
                        .GroupBy(x => x.Identifier.Text)
                        .Select(x => x.First())
                        .Where(x => !(excludeDeclaration && declaration.Contains(x.Identifier.Text)));

                    return allVars.Select(x => Argument(VariableData.GetObjectCreationSyntax(x)));
                case ExpressionStatementSyntax expressionStatementSyntax:

                    switch (expressionStatementSyntax.Expression)
                    {
                        case InvocationExpressionSyntax invocationExpressionSyntax:
                            return invocationExpressionSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>()
                                .SelectMany(x => x.Variables.Select(y => Argument(
                                        VariableData.GetObjectCreationSyntax(y)
                                    )
                                ));
                        case AssignmentExpressionSyntax assignmentExpressionSyntax:
                            if (assignmentExpressionSyntax.Left is MemberAccessExpressionSyntax)
                            {
                                return new List<ArgumentSyntax>();
                            }

                            var left = (IdentifierNameSyntax) assignmentExpressionSyntax.Left;

                            // var compilation =
                            //     CSharpCompilation.Create("temp", new[] {assignmentExpressionSyntax.SyntaxTree});

                            // if (compilation
                            //     .GetSemanticModel(assignmentExpressionSyntax.SyntaxTree)
                            //     .GetSymbolInfo(left)
                            //     .Symbol is IPropertySymbol)
                            var equivalentNode = _semanticModel.SyntaxTree.GetCompilationUnitRoot().DescendantNodes()
                                .FirstOrDefault(x =>
                                    x.IsEquivalentTo(left));
                            if (_semanticModel.GetSymbolInfo(equivalentNode).Symbol is IPropertySymbol)
                            {
                                return new List<ArgumentSyntax>();
                            }

                            return new List<ArgumentSyntax>
                            {
                                Argument(
                                    VariableData.GetObjectCreationSyntax(left)
                                )
                            };
                        default:
                            throw new NotImplementedException("Imate izraz koji još nije podržan");
                    }

                case AccessorDeclarationSyntax accessorDeclarationSyntax:

                    var args = new List<ArgumentSyntax>();

                    if (accessorDeclarationSyntax.Keyword.IsKind(SyntaxKind.GetKeyword)) return args;

                    var valueData = VariableData.GetObjectCreationSyntax(IdentifierName("value"));
                    var arg = Argument(valueData);
                    args.Add(arg);

                    return args;
                case ThrowStatementSyntax _:
                    return new List<ArgumentSyntax>();
                case ElseClauseSyntax _:
                    return new List<ArgumentSyntax>();
                case ForEachStatementSyntax forEachStatementSyntax:
                    return new List<ArgumentSyntax>
                    {
                        Argument(
                            VariableData.GetObjectCreationSyntax(
                                IdentifierName(forEachStatementSyntax.Identifier.Text))
                        )
                    };
                default:
                    throw new NotImplementedException("Imate izraz koji još nije podržan");
            }
        }

        private ArgumentSyntax CreateThisArgument()
        {
            return Argument(VariableData.GetThisReferenceVariableDataSyntax());
        }

        private string GetStatementString(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case StatementSyntax _:
                    return string.Empty;
                case ElseClauseSyntax _:
                    return string.Empty;
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return $"{methodDeclarationSyntax.Identifier.Text}";
                case ConstructorDeclarationSyntax constructorDeclarationSyntax:
                    return $"{constructorDeclarationSyntax.Identifier.Text}";
                case AccessorDeclarationSyntax accessorDeclarationSyntax:
                    // Accessor declaration syntax has AccessorList as immediate parent, skip it.
                    var property = accessorDeclarationSyntax.Parent.Parent as PropertyDeclarationSyntax;
                    Debug.Assert(property != null, "Accessor syntax should have property syntax as parent");
                    return $"{accessorDeclarationSyntax.Keyword.Text} {property.Identifier.Text}";
                default:
                    return "<expression generator unknown>";
            }
        }

        private InvocationExpressionSyntax GetMethodInvocationExpressionSyntax
        (
            ExpressionGeneratorDetails.Long details,
            ArgumentSyntax condition = null
        )
        {
            var invocationSyntax = InvocationExpression(
                GetMemberAccessExpressionSyntax(details)
            );

            var @params = GetParameters(details.InsTargetNode, details.ExcludeDeclaration).ToList();

            if (details.IncludeSelfReference)
            {
                @params.Add(CreateThisArgument());
            }

            var statementString = GetStatementString(details.InsTargetNode);
            var arguments = new SeparatedSyntaxList<ArgumentSyntax>().Add(
                Argument(
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal(details.LineData.StartLine)
                    )
                )
            );

            arguments = statementString == string.Empty
                ? arguments
                : arguments.Add(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(statementString)))
                );

            var methodInvocationSyntax = invocationSyntax.WithArgumentList(ArgumentList(arguments));

            if (condition != null)
            {
                methodInvocationSyntax = methodInvocationSyntax.AddArgumentListArguments(condition);
            }

            methodInvocationSyntax = methodInvocationSyntax.AddArgumentListArguments(@params.ToArray());

            return methodInvocationSyntax;
        }

        public ExpressionStatementSyntax GetExpressionStatement(ExpressionGeneratorDetails.Long details)
        {
            return ExpressionStatement(
                GetMethodInvocationExpressionSyntax(details)
            );
        }

        public ExpressionStatementSyntax GetConditionalExpressionStatement
        (
            ExpressionGeneratorDetails.Long details,
            ExpressionSyntax condition
        )
        {
            var conditionalArgument = Argument(
                PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                    ParenthesizedExpression(condition))
            );

            return ExpressionStatement(
                GetMethodInvocationExpressionSyntax(details, conditionalArgument)
            );
        }

        public ExpressionStatementSyntax GetDullExpressionStatement(ExpressionGeneratorDetails.Long details)
        {
            return ExpressionStatement(
                InvocationExpression(
                    GetMemberAccessExpressionSyntax(details)
                ).WithArgumentList(
                    ArgumentList(
                        new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(details.LineData.StartLine)
                                )
                            ))
                    )
                )
            );
        }

        public ExpressionStatementSyntax GetExitExpressionStatement
        (
            ExpressionGeneratorDetails.Long details,
            bool hasStatements,
            bool isMethodExit
        )
        {
            var args = new SeparatedSyntaxList<ArgumentSyntax>().Add(Argument(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(details.LineData.StartLine)
                )
            ));

            if (isMethodExit)
            {
                args = args.Add(Argument(
                        hasStatements
                            ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
                            : LiteralExpression(SyntaxKind.FalseLiteralExpression)
                    )
                );
            }

            return ExpressionStatement(
                InvocationExpression(
                    GetMemberAccessExpressionSyntax(details)
                ).WithArgumentList(
                    ArgumentList(args)
                )
            );
        }

        public LocalDeclarationStatementSyntax FromReturnToLocalDeclaration(
            string variableName,
            TypeSyntax returnType,
            ReturnStatementSyntax returnStatementSyntax)
        {
            var localDeclaration = LocalDeclarationStatement(
                VariableDeclaration(returnType,
                    new SeparatedSyntaxList<VariableDeclaratorSyntax>()
                        .Add(
                            VariableDeclarator(
                                Identifier(variableName)
                            ).WithInitializer(
                                EqualsValueClause(
                                    returnStatementSyntax.Expression
                                )
                            )
                        )
                )
            );

            return localDeclaration;
        }

        public ReturnStatementSyntax ReturnVariableStatement(string variableName)
        {
            return ReturnStatement(
                IdentifierName(variableName)
            );
        }

        public BlockSyntax WrapInBlock(params StatementSyntax[] statements)
        {
            return Block(statements);
        }

        public ConstructorDeclarationSyntax CreateStaticConstructorForClass
        (
            ClassDeclarationSyntax declarationSyntax
        )
        {
            var identifier = declarationSyntax.Identifier;
            var constructor = ConstructorDeclaration(identifier)
                .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.StaticKeyword)))
                .WithBody(Block());

            return constructor;
        }

        public ExpressionStatementSyntax GetRegisterClassLoadExpression
        (
            ExpressionGeneratorDetails egDetails,
            ConstructorDeclarationSyntax constructorDeclarationSyntax,
            string fullyQualifiedName
        )
        {
            var declarationSyntax = constructorDeclarationSyntax;
            var isStatic = declarationSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

            if (!isStatic) throw new ArgumentException("Given constructor is not static");

            return ExpressionStatement(
                InvocationExpression(
                    GetMemberAccessExpressionSyntax(egDetails),
                    ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(fullyQualifiedName)
                            )))
                        // .Add(Argument(TypeOfExpression(IdentifierName(egDetails.ClassName)))) // throws exception for fully qualified name
                    )
                )
            );
        }

        public ExpressionStatementSyntax GetRegisterLoopIteration(StatementSyntax statementSyntax)
        {
            var keyword = statementSyntax switch
            {
                ForStatementSyntax forStatementSyntax => forStatementSyntax.ForKeyword,
                WhileStatementSyntax whileStatementSyntax => whileStatementSyntax.WhileKeyword,
                DoStatementSyntax doStatementSyntax => doStatementSyntax.DoKeyword,
                _ => throw new ArgumentException("Unsupported loop exception")
            };

            var lineSpan = statementSyntax.GetLocation().GetLineSpan();
            var startLine = lineSpan.StartLinePosition;
            var endLine = lineSpan.EndLinePosition;
            var location =
                $"{startLine.Line + 1},{startLine.Character + 1}-{endLine.Line + 1}";

            var keywordArgument =
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(keyword.Text)));
            var locationArgument =
                Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(location)));

            var invocation = InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(TraceApiNames.ClassName),
                    IdentifierName(TraceApiNames.RegisterIteration))
            );

            return ExpressionStatement(
                invocation.WithArgumentList(ArgumentList(
                        new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(keywordArgument)
                            .Add(locationArgument)
                    )
                )
            );
        }

        public StatementSyntax ExtractVariableDeclaration(VariableDeclarationSyntax variableDeclarationSyntax)
        {
            return LocalDeclarationStatement(variableDeclarationSyntax);
        }

        public ExpressionSyntax StatementToExpressionSyntax(StatementSyntax statementSyntax)
        {
            if (!(statementSyntax is ExpressionStatementSyntax expressionStatement))
                throw new ArgumentException("Statement is not an Expression statement.");

            var expression = expressionStatement.Expression;
            return expression;
        }

        public FieldDeclarationSyntax GenerateCreateTraceApiInstance()
        {
            return FieldDeclaration(
                VariableDeclaration(IdentifierName("TraceApi"))
                    .WithVariables(
                        new SeparatedSyntaxList<VariableDeclaratorSyntax>().Add(
                            VariableDeclarator(Identifier("TRACEAPI"))
                                .WithInitializer(EqualsValueClause(
                                    ObjectCreationExpression(IdentifierName("TraceApi"))
                                )))
                    )
            ).WithModifiers(TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)
                )
            );
        }
    }
}