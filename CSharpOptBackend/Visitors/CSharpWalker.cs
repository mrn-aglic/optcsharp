using CSharpOptBackend.TreeManagers.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;

namespace CSharpOptBackend.Visitors
{
    public class CSharpVisitor : CSharpSyntaxVisitor
    {
        public CSharpVisitor(IDetailsManager detailsManager)
        {
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var location = node.GetLocation();
            // var lineData = new LineData();
            base.VisitMethodDeclaration(node);
        }

        public override void Visit(SyntaxNode? node)
        {
            base.Visit(node);
        }
    }
}