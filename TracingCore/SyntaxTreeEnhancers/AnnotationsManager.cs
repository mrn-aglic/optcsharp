using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using TracingCore.TreeRewriters;

namespace TracingCore.SyntaxTreeEnhancers
{
    public class AnnotationsManager
    {
        public SyntaxAnnotation OriginalLineAnnotation { get; }
        public SyntaxAnnotation LocationAnnotation { get; }
        public SyntaxAnnotation AugmentationAnnotation { get; }

        private const string OriginalLineAnnotationKind = "originalLine";
        private const string LocationAnnotationKind = "location";
        private const string AugmentationAnnotationKind = "augmentation";

        public AnnotationsManager()
        {
            OriginalLineAnnotation = new SyntaxAnnotation(OriginalLineAnnotationKind);
            LocationAnnotation = new SyntaxAnnotation(LocationAnnotationKind);
            AugmentationAnnotation = new SyntaxAnnotation(AugmentationAnnotationKind);
        }

        public (LineData startLine, LineData endLine) GetLineDataFromAnnotation(SyntaxAnnotation annotation)
        {
            if (annotation == null)
            {
                throw new ArgumentException("No annotation has been provided");
            }

            if (annotation.Kind != OriginalLineAnnotationKind)
            {
                throw new ArgumentException("Unsupported annotation kind");
            }

            var range = annotation.Data.Trim('[', ']').Split('-').Select(int.Parse).ToList();
            return (startLine: new LineData(range.First()), endLine: new LineData(range.Last()));
        }

        public string ConstructLineData(SyntaxNode node)
        {
            var lineSpan = node.GetLocation().GetLineSpan();
            return $"[{lineSpan.StartLinePosition.Line + 1}-{lineSpan.EndLinePosition.Line + 1}]";
        }

        public SyntaxAnnotation OriginalLine(SyntaxNode node)
        {
            return new SyntaxAnnotation(OriginalLineAnnotationKind, ConstructLineData(node));
        }

        public bool IsAugmentation(SyntaxNode node)
        {
            return node.HasAnnotations(AugmentationAnnotationKind);
        }

        public bool IsNotAugmentation(SyntaxNode node)
        {
            return !IsAugmentation(node);
        }
    }
}