﻿using System;
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

        Task<IDocumentationDocument> GetDocumentFor(string[] path, string documentType = "")
        {
            // Remove empty entries
            path = path.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();

            IDocumentationDocument document = null;
            var documents = DocumentationIndex.Documents.Where(w => w.Key == path[0]);

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

            document = documents.FirstOrDefault(f => f.DocumentType == GetDocumentationDocumentTypeFromString(documentType) && f.Key == finalKey);

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
