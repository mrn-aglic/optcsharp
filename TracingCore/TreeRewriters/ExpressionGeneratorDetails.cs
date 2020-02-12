using Microsoft.CodeAnalysis;

namespace TracingCore.TreeRewriters
{
    public abstract class ExpressionGeneratorDetails
    {
        public SubtreeType SubtreeType { get; }
        public string ClassName { get; }
        public string MemberName { get; }
        public LineData LineData { get; }

        public ExpressionGeneratorDetails(SubtreeType subtreeType, string className, string memberName, LineData lineData)
        {
            SubtreeType = subtreeType;
            ClassName = className;
            MemberName = memberName;
            LineData = lineData;
        }
        
        public class Short : ExpressionGeneratorDetails
        {
            public Short(SubtreeType subtreeType, string className, string memberName, LineData lineData) : base(subtreeType, className, memberName, lineData)
            {
            }
        }

        public class Long : ExpressionGeneratorDetails
        {
            public SyntaxNode BeforeNode { get; }
            public bool IncludeSelfReference { get; }

            public Long(
                SubtreeType subtreeType,
                string className,
                string memberName,
                LineData lineData,
                SyntaxNode beforeNode,
                bool includeSelfReference
            ) : base(subtreeType, className, memberName, lineData)
            {
                BeforeNode = beforeNode;
                IncludeSelfReference = includeSelfReference;
            }
        }
    }
}