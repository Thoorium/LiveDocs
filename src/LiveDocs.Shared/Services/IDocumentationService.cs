using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationService
    {
        IDocumentationIndex DocumentationIndex { get; set; }

        Task<IDocumentationIndex> IndexFiles();

        Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex);

        Task<ISearchIndex> RefreshSearchIndex(IDocumentationIndex documentationIndex);

        Task<IList<IDocumentationDocument>> GetDocumentsFor(string path)
        {
            return Task.FromResult<IList<IDocumentationDocument>>(DocumentationIndex.Documents.Where(w => w.Key == path).ToList());
        }

        Task<IDocumentationDocument> GetDocumentFor(string path, string documentType = "")
        {
            IDocumentationDocument document = null;
            var d = DocumentationIndex.Documents.Where(w => w.Key == path);

            if (string.IsNullOrWhiteSpace(documentType))
            {
                document = d.FirstOrDefault(f => f.DocumentType == DocumentationDocumentType.Markdown);
                if (document != null)
                    return Task.FromResult(document);

                document = d.FirstOrDefault(f => f.DocumentType == DocumentationDocumentType.Html);
                if (document != null)
                    return Task.FromResult(document);

                return Task.FromResult(d.FirstOrDefault());
            }

            document = d.FirstOrDefault(f => f.DocumentType == GetDocumentationDocumentTypeFromString(documentType));

            return Task.FromResult(document);
        }

        DocumentationDocumentType GetDocumentationDocumentTypeFromString(string documentationType)
        {
            return documentationType.ToLower() switch
            {
                "markdown" => DocumentationDocumentType.Markdown,
                "html" => DocumentationDocumentType.Html,
                "pdf" => DocumentationDocumentType.Pdf,
                "word" => DocumentationDocumentType.Word,
                _ => DocumentationDocumentType.Unknown
            };
        }
    }
}
