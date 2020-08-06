using LiveDocs.Shared.Services;
using System;
using System.IO;
using System.Linq;
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
        public string FileName => System.IO.Path.GetFileName(Path);
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            return await File.ReadAllTextAsync(Path);
        }
    }
}
