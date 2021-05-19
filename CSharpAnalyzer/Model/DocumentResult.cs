using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Document = Microsoft.CodeAnalysis.Document;

namespace CSharpAnalyzer.Model
{
    public class DocumentResult
    {
        public SyntaxTree SyntaxTree { get; }
        public SemanticModel SemanticModel { get; }

        public DocumentResult(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            SyntaxTree = syntaxTree;
            SemanticModel = semanticModel;
        }

        public static async Task<DocumentResult> FromDocument(Document document)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            return new DocumentResult(syntaxTree, semanticModel);
        }
    }
}