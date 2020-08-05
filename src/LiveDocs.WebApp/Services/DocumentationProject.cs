using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationProject : IDocumentationProject
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string KeyPath => _KeyPath == null ? "" : _KeyPath + "/" + Key;
        public string Path { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path) ?? "";
        public List<IDocumentationDocument> DefaultDocuments { get; set; } = new List<IDocumentationDocument>();
        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();
        public int DocumentCount => Documents.Count + SubProjects.Sum(s => s.DocumentCount);
        public List<IDocumentationProject> SubProjects { get; set; } = new List<IDocumentationProject>();
        public IDocumentationDocument LandingPage { get; set; }

        private readonly LiveDocsOptions _LiveDocsOptions;
        private readonly string _KeyPath;

        /// <summary>
        /// Create a documentation project.
        /// </summary>
        /// <param name="liveDocsOptions"></param>
        /// <param name="keyPath">Previous KeyPath to the project. If null, the current KeyPath will be empty.</param>
        public DocumentationProject(LiveDocsOptions liveDocsOptions, string keyPath)
        {
            _LiveDocsOptions = liveDocsOptions;
            _KeyPath = keyPath;
        }

        public async Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "")
        {
            var defaultDocuments = await GetDocumentationDefaultDocuments(documentType);
            return defaultDocuments.FirstOrDefault();
        }

        public async Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "")
        {
            List<IDocumentationDocument> documentationDefaultDocuments = new List<IDocumentationDocument>();

            foreach (var item in _LiveDocsOptions.DefaultDocuments)
            {
                var document = await ((IDocumentationProject)this).GetDocumentFor(new[] { item }, documentType);
                if (document != null)
                    documentationDefaultDocuments.Add(document);
            }

            return documentationDefaultDocuments.ToArray();
        }

        public async Task<IDocumentationDocument> GetDocumentationLandingPageDocument()
        {
            if (string.IsNullOrWhiteSpace(_LiveDocsOptions.LandingPageDocument))
                return null;

            return await ((IDocumentationProject)this).GetDocumentFor(new[] { _LiveDocsOptions.LandingPageDocument }, "");
        }

        public async Task<string> GetFirstAvailableDocumentPath()
        {
            return await GetFirstAvailableDocumentPath(Documents.ToArray(), KeyPath);
        }

        private async Task<string> GetFirstAvailableDocumentPath(IDocumentationDocument[] currentDocuments, string basePath)
        {
            var document = currentDocuments.FirstOrDefault(f => f.DocumentType != DocumentationDocumentType.Folder && f.DocumentType != DocumentationDocumentType.Project);

            if (document != null)
                return basePath == "/" ? $"/{document.Key}" : $"{basePath}/{document.Key}";

            foreach (var item in currentDocuments.Where(w => w.DocumentType == DocumentationDocumentType.Folder))
            {
                return await GetFirstAvailableDocumentPath(item.SubDocuments, basePath == "/" ? $"/{item.Key}" : $"{basePath}/{item.Key}");
            }

            return null;
        }
    }
}
