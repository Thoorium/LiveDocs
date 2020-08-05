using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationProject
    {
        public string Key { get; }
        public string KeyPath { get; }
        public string Name { get; }
        string Path { get; set; }
        List<IDocumentationDocument> DefaultDocuments { get; set; }
        List<IDocumentationDocument> Documents { get; set; }
        List<IDocumentationProject> SubProjects { get; set; }
        IDocumentationDocument LandingPage { get; set; }

        Task<IDocumentationDocument> GetDocumentFor(string[] path, string documentType = "")
        {
            // Remove empty entries
            path = path.Where(w => !string.IsNullOrWhiteSpace(w)).Select(s => s.ToLower()).ToArray();

            if (path.Length == 0)
                return null;

            IDocumentationDocument document = null;
            var documents = Documents.Where(w => w.Key == path[0]);

            string finalKey = path[0];

            IDocumentationDocument tempDoc = null;
            for (int i = 0; i < path.Length; i++)
            {
                if (i == path.Length - 1)
                {
                    finalKey = path[i];
                    break;
                }

                tempDoc = documents.FirstOrDefault(w => (w.DocumentType == DocumentationDocumentType.Folder || w.DocumentType == DocumentationDocumentType.Project) && w.Key == path[i]);

                if (tempDoc == null)
                    return Task.FromResult(tempDoc);

                documents = tempDoc.SubDocuments;
            }

            if (string.IsNullOrWhiteSpace(documentType))
            {
                document = documents.FirstOrDefault(f => f.DocumentType == DocumentationDocumentType.Markdown && f.Key == finalKey);
                if (document != null)
                    return Task.FromResult(document);

                document = documents.FirstOrDefault(f => f.DocumentType == DocumentationDocumentType.Html && f.Key == finalKey);
                if (document != null)
                    return Task.FromResult(document);

                document = documents.FirstOrDefault(f => f.DocumentType == DocumentationDocumentType.Pdf && f.Key == finalKey);
                if (document != null)
                    return Task.FromResult(document);

                return Task.FromResult(documents.FirstOrDefault());
            }

            document = documents.FirstOrDefault(f => f.DocumentType == DocumentationHelper.GetDocumentationDocumentTypeFromString(documentType) && f.Key == finalKey);

            return Task.FromResult(document);
        }

        Task<List<IDocumentationDocument>> GetDocumentsFor(string[] path)
        {
            // Remove empty entries
            path = path.Where(w => !string.IsNullOrWhiteSpace(w)).Select(s => s.ToLower()).ToArray();

            var documents = Documents.Where(w => w.Key == path[0]);
            string finalKey = path[0];
            IDocumentationDocument tempDoc = null;

            for (int i = 0; i < path.Length; i++)
            {
                if (i == path.Length - 1)
                {
                    finalKey = path[i];
                    break;
                }

                tempDoc = documents.FirstOrDefault(w => w.DocumentType == DocumentationDocumentType.Folder && w.Key == path[i]);

                if (tempDoc == null)
                    return Task.FromResult(new List<IDocumentationDocument>());

                documents = tempDoc.SubDocuments;
            }

            return Task.FromResult(documents.Where(w => w.Key == finalKey && w.DocumentType != DocumentationDocumentType.Folder).ToList());
        }

        Task<IDocumentationDocument> GetDocumentByFileName(string[] path)
        {
            // Remove empty entries
            path = path.Where(w => !string.IsNullOrWhiteSpace(w)).Select(s => s.ToLower()).ToArray();

            if (path.Length == 0)
                return null;

            IDocumentationDocument document = null;
            var documents = Documents.Where(w => w.FileName.Equals(path[0], System.StringComparison.InvariantCultureIgnoreCase) || w.Key == path[0]);

            string finalKey = path[0];

            IDocumentationDocument tempDoc = null;
            for (int i = 0; i < path.Length; i++)
            {
                if (i == path.Length - 1)
                {
                    finalKey = path[i];
                    break;
                }

                tempDoc = documents.FirstOrDefault(w => (w.DocumentType == DocumentationDocumentType.Folder || w.DocumentType == DocumentationDocumentType.Project) && w.Key == path[i]);

                if (tempDoc == null)
                    return Task.FromResult(tempDoc);

                documents = tempDoc.SubDocuments;
            }

            document = documents.FirstOrDefault(f => f.FileName.Equals(finalKey, System.StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(document);
        }

        Task<string> GetFirstAvailableDocumentPath();

        Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "");
        Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "");
        Task<IDocumentationDocument> GetDocumentationLandingPageDocument();
    }
}
