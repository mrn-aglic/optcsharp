using System;
using System.Collections.Generic;
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
        private MemberAccessExpressionSyntax GetMemberAccessExpressionSyntax(ExpressionGeneratorDetails details)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(details.ClassName),
                IdentifierName(details.MemberName)
            );
        }

        private IEnumerable<ArgumentSyntax> GetParameters(SyntaxNode syntaxNode)
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
                            return new List<ArgumentSyntax>
                            {
                                Argument(
                                    VariableData.GetObjectCreationSyntax(left)
                                )
                            };
                        default:
                            throw new NotImplementedException("Imate izraz koji još nije podržan");
                    }

                case AccessorDeclarationSyntax _:
                    return new List<ArgumentSyntax>();
                case ThrowStatementSyntax _:
                    return new List<ArgumentSyntax>();
                default:
                    throw new NotSupportedException();
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
                case StatementSyntax statementSyntax:
                    return statementSyntax.ToString();
                case MethodDeclarationSyntax methodDeclarationSyntax:
                    return $"{methodDeclarationSyntax.Identifier.Text}";
                case ConstructorDeclarationSyntax constructorDeclarationSyntax:
                    return $"{constructorDeclarationSyntax.Identifier.Text}";
                default:
                    return "<expression generator unknown>";
            }
        }

        private InvocationExpressionSyntax GetMethodInvocationExpressionSyntax(ExpressionGeneratorDetails.Long details)
        {
            var invocationSyntax = InvocationExpression(
                GetMemberAccessExpressionSyntax(details)
            );

            var @params = GetParameters(details.InsTargetNode).ToList();
            var statementString = GetStatementString(details.InsTargetNode);

            if (details.IncludeSelfReference)
            {
                @params.Add(CreateThisArgument());
            }

            var methodInvocationSyntax = invocationSyntax.WithArgumentList(ArgumentList(
                new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(details.LineData.StartLine)
                        )
                    ))
                    .Add(Argument(
                        LiteralExpression(SyntaxKind.StringLiteralExpression,
                            Literal(statementString))
                    ))
            )).AddArgumentListArguments(@params.ToArray());

            return methodInvocationSyntax;
        }

        public ExpressionStatementSyntax GetExpressionStatement(ExpressionGeneratorDetails.Long details)
        {
            return ExpressionStatement(
                GetMethodInvocationExpressionSyntax(details)
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
                            .Add(Argument(
                                LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal(details.InsTargetNode.ToString())
                                )
                            ))
                    )
                )
            );
        }

        public AttributeListSyntax CreateAttribute(
            int line,
            string name)
        {
            var attributeArgument = AttributeArgument(
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(line))
            );

            var attribute = Attribute(
                IdentifierName(name),
                AttributeArgumentList(
                    new SeparatedSyntaxList<AttributeArgumentSyntax>().Add(attributeArgument)
                )
            );

            return AttributeList(new SeparatedSyntaxList<AttributeSyntax>().Add(attribute));
        }

        public ExpressionStatementSyntax GetExitExpressionStatement(ExpressionGeneratorDetails.Long details)
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
                            .Add(Argument(
                                LiteralExpression(SyntaxKind.StringLiteralExpression,
                                    Literal(details.InsTargetNode.ToString())
                                )
                            ))
                            .Add(Argument(
                                    LiteralExpression(SyntaxKind.NullLiteralExpression)
                                )
                            )
                    )
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
                    )
                )
            );
        }
    }
}