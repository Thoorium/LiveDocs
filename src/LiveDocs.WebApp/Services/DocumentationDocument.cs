using LiveDocs.Shared.Services;
using System;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationDocument : IDocumentationDocument
    {
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }
        public DocumentationDocumentType DocumentType { get; set; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public IDocumentationDocument[] SubDocuments { get; set; } = null;
    }
}
