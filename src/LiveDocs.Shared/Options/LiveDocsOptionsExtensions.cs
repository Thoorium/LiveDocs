using System.IO;

namespace LiveDocs.Shared.Options
{
    public static class LiveDocsOptionsExtensions
    {
        public static DirectoryInfo GetDocumentationFolderAsAbsolute(this LiveDocsOptions liveDocsOptions, string contentRootPath)
        {
            DirectoryInfo directoryInfo;
            if (Path.IsPathRooted(liveDocsOptions.DocumentationFolder))
                directoryInfo = new DirectoryInfo(liveDocsOptions.DocumentationFolder);
            else directoryInfo = new DirectoryInfo(Path.Combine(contentRootPath, liveDocsOptions.DocumentationFolder));

            return directoryInfo;
        }
    }
}
