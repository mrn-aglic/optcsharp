using System.Collections.Generic;
using System.Linq;
using CSharpOptBackend.Interfaces;
using CSharpOptBackend.SyntaxComposers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynExtensions.Extensions;
using RoslynExtensions.Models;

namespace CSharpOptBackend.DebuggerCommands
{
    public class DebuggerCommand : ICommand
    {
        public const string DebuggerName = "SharpTutor";
        public const string Step = "Step";

        private readonly SyntaxNode _syntaxNode;
        private readonly IEnumerable<string> _variables;

        public DebuggerCommand(SyntaxNode syntaxNode, IEnumerable<string> variables)
        {
            _syntaxNode = syntaxNode;
            // _span = span;
            _variables = variables;
        }

        public string GetStepCommand()
        {
            var span = _syntaxNode.GetSpan();
            var spanDetails = $"{span.GetSingleLineHighlight()}";
            var paramsText = ToParamsText(_variables);
            return $"{DebuggerName}.{Step}({spanDetails}, {paramsText})";
        }

        private string ToParamsText(IEnumerable<string> variables)
        {
            return string.Join(",", variables.Select(NewVarTemplate));
        }

        private string NewVarTemplate(string v)
        {
            return $"new Var(\"{v}\",{v})";
        }

        public InvocationExpressionSyntax GetStepCommandTree()
        {
            var args = CreateArgumentList();
            var memberAccess =
                Expression.GetMemberInvocationExpression(DebuggerName, Step)
                    .WithArgumentList(args);
            return memberAccess;
        }

        public ArgumentListSyntax CreateArgumentList()
        {
            var span = _syntaxNode.GetSpan();

            var spanExpression = CommandParts.GetHighlightSpan(span.GetSingleLineHighlight());
            var vars = _variables.Select(Var.GetVar);
            var args = vars.Prepend(spanExpression);
            return ArgumentList.CreateArgumentList(args.ToArray());
        }
    }
}