using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TracingCore.Common;
using TracingCore.TreeRewriters;

namespace TracingCore.SourceCodeInstrumentation
{
    public class ClassInstrumentation
    {
        private readonly InstrumentationShared _instrumentationShared;
        private readonly ExpressionGenerator _expressionGenerator;
        private readonly bool _ignoreProgramClass = true;

        public ClassInstrumentation(InstrumentationShared instrumentationShared,
            ExpressionGenerator expressionGenerator)
        {
            _instrumentationShared = instrumentationShared;
            _expressionGenerator = expressionGenerator;
        }

        public InstrumentationData PrepareTraceData(CompilationUnitSyntax root)
        {
            var insData = new List<InstrumentationData>();
            var declarations = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().ToList();
            foreach (var declarationSyntax in declarations)
            {
                var data = PrepareTraceData(declarationSyntax, root);
                insData.Add(data);
            }

            return new InstrumentationData(root, insData.SelectMany(x => x.Statements).ToList());
        }

        public InstrumentationData PrepareTraceData(ConstructorDeclarationSyntax constructorDeclarationSyntax,
            CompilationUnitSyntax root)
        {
            return _instrumentationShared.GetMethodInsData(constructorDeclarationSyntax, root);
        }

        public InstrumentationData PrepareStaticConstructor(CompilationUnitSyntax root)
        {
            return new InstrumentationData
            (
                root,
                root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(x => !_ignoreProgramClass || x.Identifier.Text != "Program")
                    .Select(PrepareStaticConstructor).ToList()
            );
        }

        public InstrumentationDetails PrepareStaticConstructor(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var staticConstructor = classDeclarationSyntax.ChildNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .FirstOrDefault(x => x.Modifiers.Any(y => y.IsKind(SyntaxKind.StaticKeyword)));
            var hasStaticConstructor = staticConstructor != null && staticConstructor.Body.Statements.Any();

            var lineData = RoslynHelper.GetLineData(classDeclarationSyntax);
            var egDetails = new ExpressionGeneratorDetails.Short(
                SubtreeType.MethodInvocation,
                TraceApiNames.ClassName,
                TraceApiNames.RegisterClassLoad,
                lineData
            );

            var staticCotr = hasStaticConstructor
                ? staticConstructor
                : _expressionGenerator.CreateStaticConstructorForClass(classDeclarationSyntax);
            var fullyQualifiedName = RoslynHelper.GetClassParentPath(classDeclarationSyntax);
            var registerStatement = _expressionGenerator.GetRegisterClassLoadExpression(egDetails, staticCotr, fullyQualifiedName);

            return hasStaticConstructor
                ? new InstrumentationDetails(
                    staticCotr.Body.Statements.First(), registerStatement, Insert.Before)
                : new InstrumentationDetails(
                    classDeclarationSyntax, staticCotr.WithBody(staticCotr.Body.AddStatements(registerStatement)), Insert.Member);
        }
    }
}