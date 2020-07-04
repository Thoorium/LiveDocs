using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Options
{
    public static class LiveDocsOptionsExtensions
    {

        public static DirectoryInfo GetDocumentationFolderAsAbsolute(this LiveDocsOptions liveDocsOptions, string root)
        {
            DirectoryInfo directoryInfo;
            if (Path.IsPathRooted(liveDocsOptions.DocumentationFolder))
                directoryInfo = new DirectoryInfo(liveDocsOptions.DocumentationFolder);
            else directoryInfo = new DirectoryInfo(Path.Combine(root, liveDocsOptions.DocumentationFolder));

            return directoryInfo;
        }
    }
}
