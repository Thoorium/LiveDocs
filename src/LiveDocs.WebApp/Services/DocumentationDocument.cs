using LiveDocs.Shared.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationDocument : IDocumentationDocument
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }
        public DocumentationDocumentType DocumentType { get; set; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public IDocumentationDocument[] SubDocuments { get; set; } = null;

        public async Task<string> ToHtml(string baseUri = "")
        {
            return await File.ReadAllTextAsync(Path);
        }
    }
}
