using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.SyntaxTreeEnhancers;
using TracingCore.TreeRewriters;
using static TracingCore.Common.RoslynHelper;

namespace TracingCore.RoslynRewriters
{
    public class RewriteHelper
    {
        private readonly ExpressionGenerator _expressionGenerator;
        private readonly AnnotationsManager _annotationsManager;

        public RewriteHelper(ExpressionGenerator expressionGenerator)
        {
            _expressionGenerator = expressionGenerator;
            _annotationsManager = new AnnotationsManager();
        }

        internal List<StatementSyntax> UnwrapAugmentedBlock(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is BlockSyntax)) return new List<StatementSyntax> {syntaxNode as StatementSyntax};

            var block = syntaxNode as BlockSyntax;
            return block.HasAnnotation(_annotationsManager.AugmentationAnnotation) //&& block.Parent is BlockSyntax
                ? block.Statements.ToList()
                : new List<StatementSyntax> {block};
        }
        
        internal BlockSyntax WrapInBlock(params StatementSyntax[] statements)
        {
            return _expressionGenerator.WrapInBlock(statements)
                .WithAdditionalAnnotations(_annotationsManager.AugmentationAnnotation);
        }

        internal ConstructorDeclarationSyntax PrepareStaticConstructor(ClassDeclarationSyntax classDeclarationSyntax)
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
    }
}