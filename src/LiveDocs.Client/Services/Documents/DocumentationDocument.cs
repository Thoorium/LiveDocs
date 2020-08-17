﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Remote;

namespace LiveDocs.Client.Services.Documents
{
    public class DocumentationDocument : IRemoteDocumentationDocument
    {
        public DocumentationDocumentType DocumentType { get; set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Task<string> GetTitle(HttpClient httpClient)
        {
            return Task.FromResult("");
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryCache()
        {
            return Task.FromResult(true);
        }

        public Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            throw new NotImplementedException();
        }
    }
}