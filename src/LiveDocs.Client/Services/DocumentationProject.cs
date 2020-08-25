using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Shared;
using LiveDocs.Shared.Services;

namespace LiveDocs.Client.Services
{
    public class DocumentationProject : IDocumentationProject
    {
        public DocumentationProject()
        {
        }

        public List<IDocumentationDocument> DefaultDocuments { get; set; }
        public int DocumentCount => Documents.Count(c => c.DocumentType != DocumentationDocumentType.Folder && c.DocumentType != DocumentationDocumentType.Project) + SubProjects.Sum(s => s.DocumentCount) + Documents.Sum(s => s.SubDocumentsCount);
        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();
        public string Key => UrlHelper.Urilize(Name);
        public string KeyPath { get; set; }
        public IDocumentationDocument LandingPage { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path) ?? "";
        public string Path { get; set; }
        public List<IDocumentationProject> SubProjects { get; set; } = new List<IDocumentationProject>();
        public async Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "")
        {
            var defaultDocuments = await GetDocumentationDefaultDocuments(documentType);
            return defaultDocuments.FirstOrDefault();
        }

        public Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "")
        {
            return Task.FromResult(DefaultDocuments?.ToArray());
        }

        public async Task<IDocumentationDocument> GetDocumentationLandingPageDocument()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetFirstAvailableDocumentPath()
        {
            return await GetFirstAvailableDocumentPath(Documents.ToArray(), KeyPath);
        }

        private async Task<string> GetFirstAvailableDocumentPath(IDocumentationDocument[] currentDocuments, string basePath)
        {
            var document = currentDocuments.FirstOrDefault(f => f.DocumentType != DocumentationDocumentType.Folder && f.DocumentType != DocumentationDocumentType.Project);

            if (document != null)
                return $"{basePath}/{document.Key}";

            foreach (var item in currentDocuments.Where(w => w.DocumentType == DocumentationDocumentType.Folder))
            {
                return await GetFirstAvailableDocumentPath(item.SubDocuments, $"{basePath}/{item.Key}");
            }

            return null;
        }
    }
}