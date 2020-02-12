using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Data;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TracingCore.TraceToPyDtos
{
    public class VariableData : ITracePyDto
    {
        private const string ThisClassName = "VariableData";
        public string Name { get; }
        public object Value { get; }
        public Type Type { get; }
        public bool IsValueType { get; }

        public VariableData(string name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
            IsValueType = value != null && value.GetType().IsValueType;
        }

        public static VariableData EmptyVariableData()
        {
            return new VariableData(string.Empty, null, null);
        }

        private static ArgumentListSyntax GetArgumentList
        (
            SyntaxToken identifier,
            TypeSyntax type,
            bool hasInit = true
        )
        {
            var secondArgument = hasInit
                ? Argument(IdentifierName(identifier))
                : Argument(
                    LiteralExpression(SyntaxKind.StringLiteralExpression,
                        Literal("<nije inic>"))
                );
            var thirdArgument = hasInit
                ? Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(identifier),
                            IdentifierName("GetType")
                        )
                    )
                )
                : Argument(TypeOfExpression(type));

            return ArgumentList(
                new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(
                        Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(identifier.Text)
                            )
                        )
                    )
                    .Add(secondArgument)
                    .Add(thirdArgument)
            );
        }

        public static ObjectCreationExpressionSyntax GetObjectCreationSyntax(
            VariableDeclaratorSyntax variableDeclaratorSyntax)
        {
            var variableDeclaration = variableDeclaratorSyntax.Parent as VariableDeclarationSyntax;
            var a = ObjectCreationExpression(
                IdentifierName(ThisClassName)
            ).WithArgumentList(
                GetArgumentList(
                    variableDeclaratorSyntax.Identifier,
                    variableDeclaration.Type,
                    variableDeclaratorSyntax.Initializer != null)
            ).NormalizeWhitespace();

            return a;
        }

        public static ObjectCreationExpressionSyntax GetObjectCreationSyntax
        (
            ParameterSyntax parameterSyntax
        )
        {
            var a = ObjectCreationExpression(
                IdentifierName(ThisClassName)
            ).WithArgumentList(
                GetArgumentList(parameterSyntax.Identifier, parameterSyntax.Type)
            ).NormalizeWhitespace();

            return a;
        }

        // We do not know the type of the identifier
        public static ObjectCreationExpressionSyntax GetObjectCreationSyntax
        (
            IdentifierNameSyntax identifierName,
            TypeSyntax type = null
        )
        {
            var a = ObjectCreationExpression(
                IdentifierName(ThisClassName)
            ).WithArgumentList(
                GetArgumentList(identifierName.Identifier, type)
            ).NormalizeWhitespace();

            return a;
        }

        public static InvocationExpressionSyntax GetThisWithMemberwiseClone()
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName("MemberwiseClone")
                )
            );
        }

        public static ObjectCreationExpressionSyntax GetThisReferenceVariableDataSyntax()
        {
            var a = ObjectCreationExpression(
                IdentifierName(ThisClassName)
            ).WithArgumentList(
                ArgumentList(
                    new SeparatedSyntaxList<ArgumentSyntax>()
                        .Add(
                            // Argument(GetThisWithMemberWiseClone())
                            Argument(LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("this")
                                )
                            )
                        )
                        .Add(
                            Argument(
                                // GetThisWithMemberwiseClone()
                                ThisExpression()
                            )
                        )
                        .Add(
                            Argument(InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName("GetType")
                                )
                            ))
                        ))
            ).NormalizeWhitespace();

            return a;
        }

        public HeapData ToHeapData()
        {
            throw new NotImplementedException();
        }
    }
}