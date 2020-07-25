﻿using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents;
using LiveDocs.WebApp.Options;
using Markdig;
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
        private readonly MarkdownPipeline _MarkdownPipeline;

        public IDocumentationIndex DocumentationIndex { get; set; }

        public DocumentationService(ILogger<DocumentationService> logger, IOptions<LiveDocsOptions> options, IWebHostEnvironment hostingEnvironment, MarkdownPipeline markdownPipeline)
        {
            _Logger = logger;
            _Options = options.Value;
            _HostingEnvironment = hostingEnvironment;
            _MarkdownPipeline = markdownPipeline;
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

                switch (docType)
                {
                    case DocumentationDocumentType.Markdown:
                        documents.Add(new MarkdownDocument(_MarkdownPipeline)
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;
                    case DocumentationDocumentType.Pdf:
                        documents.Add(new PdfDocument
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;
                    case DocumentationDocumentType.Html:
                        documents.Add(new HtmlDocument
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;
                    case DocumentationDocumentType.Word:
                    case DocumentationDocumentType.Folder:
                        documents.Add(new DocumentationDocument
                        {
                            Path = file.FullName,
                            DocumentType = docType,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;
                    case DocumentationDocumentType.Unknown:
                        break;
                    default:
                        break;
                }
            }

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                var documentationDirectory = new DocumentationDocument
                {
                    DocumentType = DocumentationDocumentType.Folder,
                    Path = directory.FullName,
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

        public async Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex)
        {
            if (DocumentationIndex == null)
                _Logger.LogInformation($"Initializing documentation index. {documentationIndex.Documents.Count} documents added.");
            else _Logger.LogInformation($"Refreshing documentation index. Before {DocumentationIndex.Documents.Count}; After {documentationIndex.Documents.Count}.");

            DocumentationIndex = documentationIndex;

            DocumentationIndex.LandingPage = await GetDocumentationLandingPageDocument();

            DocumentationIndex.DefaultDocuments.Clear();
            DocumentationIndex.DefaultDocuments.AddRange(await GetDocumentationDefaultDocuments());
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

        public async Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "")
        {
            var defaultDocuments = await GetDocumentationDefaultDocuments(documentType);
            return defaultDocuments.FirstOrDefault();
        }

        public async Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "")
        {
            List<IDocumentationDocument> documentationDefaultDocuments = new List<IDocumentationDocument>();

            foreach (var item in _Options.DefaultDocuments)
            {
                var document = await ((IDocumentationService)this).GetDocumentFor(new[] { item }, documentType);
                if(document != null)
                    documentationDefaultDocuments.Add(document);
            }

            return documentationDefaultDocuments.ToArray();
        }

        public async Task<IDocumentationDocument> GetDocumentationLandingPageDocument()
        {
            if (string.IsNullOrWhiteSpace(_Options.LandingPageDocument))
                return null;

            return await ((IDocumentationService)this).GetDocumentFor(new[] { _Options.LandingPageDocument }, "");
        }
    }
}
