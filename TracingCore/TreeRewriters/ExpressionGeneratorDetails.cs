using Microsoft.CodeAnalysis;

namespace TracingCore.TreeRewriters
{
    public abstract class ExpressionGeneratorDetails
    {
        public string InstanceName { get; }
        public string ClassName { get; }
        public string MemberName { get; }
        public LineData LineData { get; }

        public ExpressionGeneratorDetails(string className, string memberName, LineData lineData)
        {
            ClassName = className;
            MemberName = memberName;
            LineData = lineData;
        }

        public class Short : ExpressionGeneratorDetails
        {
            public Short(string className, string memberName, LineData lineData) : base(className, memberName, lineData)
            {
            }
        }

        public class Long : ExpressionGeneratorDetails
        {
            public SyntaxNode InsTargetNode { get; }
            public bool IncludeSelfReference { get; }
            public bool ExcludeDeclaration { get; }

            public Long(
                string className,
                string memberName,
                LineData lineData,
                SyntaxNode insTargetNode,
                bool includeSelfReference
            ) : base(className, memberName, lineData)
            {
                InsTargetNode = insTargetNode;
                IncludeSelfReference = includeSelfReference;
            }

            public Long(
                string className,
                string memberName,
                LineData lineData,
                SyntaxNode insTargetNode,
                bool includeSelfReference,
                bool excludeDeclaration
            ) : base(className, memberName, lineData)
            {
                InsTargetNode = insTargetNode;
                IncludeSelfReference = includeSelfReference;
                ExcludeDeclaration = excludeDeclaration;
            }
        }
    }
}