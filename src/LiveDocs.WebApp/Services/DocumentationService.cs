﻿using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationService : IDocumentationService
    {
        private readonly ILogger<DocumentationService> _Logger;
        private readonly LiveDocsOptions _Options;
        private readonly IWebHostEnvironment _HostingEnvironment;

        public IDocumentationIndex DocumentationIndex { get; set; }

        public DocumentationService(ILogger<DocumentationService> logger, IOptions<LiveDocsOptions> options, IWebHostEnvironment hostingEnvironment)
        {
            _Logger = logger;
            _Options = options.Value;
            _HostingEnvironment = hostingEnvironment;
        }

        public Task<IDocumentationIndex> IndexFiles()
        {
            DirectoryInfo directoryInfo = _Options.GetDocumentationFolderAsAbsolute(_HostingEnvironment.ContentRootPath);

            IDocumentationIndex documentationIndex = new DocumentationIndex();

            var documents = BuildDocumentationSubTree(directoryInfo, directoryInfo);

            documentationIndex.Documents.AddRange(documents);

            return Task.FromResult(documentationIndex);
        }

        private List<IDocumentationDocument> BuildDocumentationSubTree(DirectoryInfo directoryInfo, DirectoryInfo topDirectoryInfo)
        {
            List<IDocumentationDocument> documents = new List<IDocumentationDocument>();

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                var docType = GetDocumentType(Path.GetExtension(file.FullName));

                if (docType != DocumentationDocumentType.Unknown)
                {
                    documents.Add(new DocumentationDocument
                    {
                        Path = file.FullName,//.Replace(topDirectoryInfo.FullName + "\\", "").Replace("\\", "/"),
                        DocumentType = docType,
                        LastUpdate = file.LastWriteTimeUtc
                    });
                }
            }

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                var documentationDirectory = new DocumentationDocument
                {
                    DocumentType = DocumentationDocumentType.Folder,
                    Path = directory.FullName,//.Replace(topDirectoryInfo.FullName + "\\", "").Replace("\\", "/"),
                    LastUpdate = directory.LastWriteTimeUtc
                };
                var subdocuments = BuildDocumentationSubTree(directory, topDirectoryInfo);

                if(subdocuments.Any())
                {
                    documentationDirectory.SubDocuments = subdocuments.ToArray();
                    documents.Add(documentationDirectory);
                }                
            }

            return documents.OrderBy(o => o.Name).ToList();
        }

        public Task<ISearchIndex> RefreshSearchIndex(IDocumentationIndex documentationIndex)
        {
            //throw new NotImplementedException();
            return default;
        }

        public Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex)
        {
            DocumentationIndex = documentationIndex;
            return Task.CompletedTask;
        }

        private DocumentationDocumentType GetDocumentType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension) || extension == ".")
                return DocumentationDocumentType.Unknown;

            extension = extension[1..].ToLower();

            return extension switch
            {
                "htm" => DocumentationDocumentType.Html,
                "html" => DocumentationDocumentType.Html,
                "md" => DocumentationDocumentType.Markdown,
                "pdf" => DocumentationDocumentType.Pdf,
                "doc" => DocumentationDocumentType.Word,
                "docx" => DocumentationDocumentType.Word,
                _ => DocumentationDocumentType.Unknown
            };
        }
            
    }
}
