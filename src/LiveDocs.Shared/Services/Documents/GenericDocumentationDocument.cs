using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public class GenericDocumentationDocument : IDocumentationDocument
    {
        public DocumentationDocumentType DocumentType { get; set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            return await File.ReadAllTextAsync(Path);
        }
    }
}