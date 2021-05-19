using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpAnalyzer.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace CSharpAnalyzer
{
    public class ProjectLoader
    {
        const string Path = "/Users/marinagliccuvic/RiderProjects/OptLocal/CSharpAnalyzer/outputprojects";

        private string DirectoryCopy(string solutionPath)
        {
            var file = new FileInfo(solutionPath);
            var solutionSrc = file.Directory?.FullName;
            var slnName = file.Name.Substring(0, file.Name.IndexOf(".sln", StringComparison.Ordinal));
            var outputPath = DirectoryCopy(solutionSrc, System.IO.Path.Combine(Path, slnName));
            return System.IO.Path.Combine(outputPath, file.Name);
        }

        private string DirectoryCopy(string sourceDirName, string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Source directory does not exist or could not be found: {sourceDirName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (var fileInfo in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, fileInfo.Name);
                fileInfo.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo directoryInfo in dirs)
            {
                string tempPath = System.IO.Path.Combine(destDirName, directoryInfo.Name);
                DirectoryCopy(directoryInfo.FullName, tempPath);
            }

            return destDirName;
        }

        public async Task<DocumentResult[]> LoadSolution(string solutionPath)
        {
            var outputPath = DirectoryCopy(solutionPath);
            Console.WriteLine(outputPath);

            using var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(outputPath);

            var docs = solution.Projects
                .SelectMany(p => p.Documents).ToList();

            var documentResultTasks = docs.Select(DocumentResult.FromDocument);
            var documentResults = await Task.WhenAll(documentResultTasks.ToArray());
            return documentResults;
        }

        public void SaveSolution(Document workspace)
        {
        }
    }
}