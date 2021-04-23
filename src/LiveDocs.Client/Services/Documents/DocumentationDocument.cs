using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents;
using LiveDocs.Shared.Services.Remote;

namespace LiveDocs.Client.Services.Documents
{
    public class DocumentationDocument : IRemoteDocumentationDocument
    {
        public DocumentationDocumentType DocumentType { get; set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => DocumentationHelper.GetDocumentNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Task<List<DocumentTreeItem>> GetDocumentTree()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTitle(HttpClient httpClient)
        {
            return Task.FromResult("");
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            throw new NotImplementedException();
        }

        public Task<DocumentCacheResult> TryCache()
        {
            return Task.FromResult(DocumentCacheResult.Success);
        }

        public Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            throw new NotImplementedException();
        }
    }
}