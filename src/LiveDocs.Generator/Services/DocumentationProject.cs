using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Shared;
using LiveDocs.Shared.Services;

namespace LiveDocs.Generator.Services
{
    public class DocumentationProject : IDocumentationProject
    {
        private readonly string[] _DefaultDocuments;
        private readonly string _KeyPath;
        private readonly string _LandingPageDocument;
        private string keyPath = null;

        /// <summary>
        /// Create a documentation project.
        /// </summary>
        /// <param name="liveDocsOptions"></param>
        /// <param name="keyPath">Previous KeyPath to the project. If null, the current KeyPath will be empty.</param>
        public DocumentationProject(string[] defaultDocuments, string landingPageDocument, string keyPath)
        {
            _DefaultDocuments = defaultDocuments;
            _LandingPageDocument = landingPageDocument;
            _KeyPath = keyPath;
        }

        public List<IDocumentationDocument> DefaultDocuments { get; set; } = new List<IDocumentationDocument>();
        public int DocumentCount => Documents.Count(c => c.DocumentType != DocumentationDocumentType.Folder && c.DocumentType != DocumentationDocumentType.Project) + SubProjects.Sum(s => s.DocumentCount) + Documents.Sum(s => s.SubDocumentsCount);
        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();
        public string Key => UrlHelper.Urilize(Name);

        public string KeyPath
        {
            get
            {
                return keyPath ?? (_KeyPath == null ? "" : (_KeyPath + "/" + Key).Trim('/'));
            }
            set
            {
                keyPath = value;
            }
        }

        public IDocumentationDocument LandingPage { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path) ?? "";
        public string Path { get; set; }
        public List<IDocumentationProject> SubProjects { get; set; } = new List<IDocumentationProject>();

        public async Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "")
        {
            var defaultDocuments = await GetDocumentationDefaultDocuments(documentType);
            return defaultDocuments.FirstOrDefault();
        }

        public async Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "")
        {
            List<IDocumentationDocument> documentationDefaultDocuments = new List<IDocumentationDocument>();

            if (_DefaultDocuments == null)
                return documentationDefaultDocuments.ToArray();

            foreach (var item in _DefaultDocuments)
            {
                var document = await ((IDocumentationProject)this).GetDocumentFor(new[] { item }, documentType);
                if (document != null)
                    documentationDefaultDocuments.Add(document);
            }

            return documentationDefaultDocuments.ToArray();
        }

        public async Task<IDocumentationDocument> GetDocumentationLandingPageDocument()
        {
            if (string.IsNullOrWhiteSpace(_LandingPageDocument))
                return null;

            return await ((IDocumentationProject)this).GetDocumentFor(new[] { _LandingPageDocument }, "");
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