﻿using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationProject : IDocumentationProject
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string Path { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path) ?? "";
        public List<IDocumentationDocument> DefaultDocuments { get; set; } = new List<IDocumentationDocument>();
        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();
        public List<IDocumentationProject> SubProjects { get; set; } = new List<IDocumentationProject>();
        public IDocumentationDocument LandingPage { get; set; }

        private readonly LiveDocsOptions _LiveDocsOptions;

        public DocumentationProject(LiveDocsOptions liveDocsOptions)
        {
            _LiveDocsOptions = liveDocsOptions;
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
    }
}