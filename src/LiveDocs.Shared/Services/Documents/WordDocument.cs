using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mammoth;

namespace LiveDocs.Shared.Services.Documents
{
    public class WordDocument : IDocumentationDocument
    {
        public virtual byte[] Data => File.ReadAllBytes(Path);
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Markdown;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;
        protected virtual DocumentConverter wordConverter => new DocumentConverter();

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            using MemoryStream stream = new MemoryStream(Data);
            stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult(wordConverter.ConvertToHtml(stream).Value);
        }
    }
}