using System;
using System.IO;

namespace TracingCore.Common
{
    public static class FileIO
    {
        public static void WriteToFile(string contents, string dirName, string fileName, bool useBasePath = true)
        {
            var basePath = useBasePath
                ? AppContext.BaseDirectory.Substring(0,
                    AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal))
                : string.Empty;

            if (!Directory.Exists($"{basePath}/{dirName}")) Directory.CreateDirectory($"{basePath}/{dirName}");
            File.WriteAllText($"{basePath}/{dirName}/{fileName}", contents);
        }
    }
}