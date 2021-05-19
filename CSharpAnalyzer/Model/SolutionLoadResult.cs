using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CSharpAnalyzer.Model
{
    public class SolutionLoadResult
    {
        private MSBuildWorkspace _workspace;
        private Solution _solution;
        public DocumentResult[] DocumentResults { get; }

        public SolutionLoadResult(MSBuildWorkspace workspace, Solution solution, DocumentResult[] documentResults)
        {
            _workspace = workspace;
            _solution = solution;
            DocumentResults = documentResults;
        }

        public void SaveToDirectory(string outputPath)
        {
            
            _workspace.TryApplyChanges(_solution);
        }
    }
}