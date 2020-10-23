using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteDocumentationIndex
    {
        public RemoteDocumentationIndex(IDocumentationIndex localDocumentationIndex, string documentationRootPath)
        {
            DefaultProject = new RemoteDocumentationProject();
            DefaultProject.Documents = CopyDocuments(localDocumentationIndex.DefaultProject.Documents, documentationRootPath).ToList();
            DefaultProject.DefaultDocuments = CopyDocuments(localDocumentationIndex.DefaultProject.DefaultDocuments, documentationRootPath).ToList();
            if (localDocumentationIndex.DefaultProject.LandingPage != null)
            {
                DefaultProject.LandingPage = new RemoteDocumentationDocument
                {
                    Path = localDocumentationIndex.DefaultProject.LandingPage.Path.Replace(documentationRootPath, "").Replace("\\", "/"),
                    LastUpdate = localDocumentationIndex.DefaultProject.LandingPage.LastUpdate
                };
            }

            Projects = new List<RemoteDocumentationProject>();
            foreach (var localProject in localDocumentationIndex.Projects)
            {
                RemoteDocumentationProject project = new RemoteDocumentationProject();
                project.Path = localProject.Path.Replace(documentationRootPath, "").Replace("\\", "/");
                project.KeyPath = localProject.KeyPath;
                project.Documents = CopyDocuments(localProject.Documents, documentationRootPath).ToList();
                project.DefaultDocuments = CopyDocuments(localProject.DefaultDocuments, documentationRootPath).ToList();

                if (localProject.LandingPage != null)
                {
                    project.LandingPage = new RemoteDocumentationDocument
                    {
                        Path = localProject.LandingPage.Path.Replace(documentationRootPath, "").Replace("\\", "/"),
                        LastUpdate = localProject.LandingPage.LastUpdate
                    };
                }

                Projects.Add(project);
            }
        }

        public RemoteDocumentationIndex()
        {
        }

        public RemoteDocumentationProject DefaultProject { get; set; }
        public List<RemoteDocumentationProject> Projects { get; set; }

        public TDocumentationIndex ToDocumentationIndex<TDocumentationIndex, TDocumentationProject, TDocumentationDocument>(IServiceProvider serviceProvider) where TDocumentationIndex : IDocumentationIndex, new() where TDocumentationProject : IDocumentationProject, new() where TDocumentationDocument : IDocumentationDocument, new()
        {
            TDocumentationIndex documentationIndex = new TDocumentationIndex();
            documentationIndex.DefaultProject = new TDocumentationProject();
            documentationIndex.DefaultProject.Documents = CopyDocuments<TDocumentationDocument>(DefaultProject.Documents, serviceProvider).ToList();
            documentationIndex.DefaultProject.DefaultDocuments = CopyDocuments<TDocumentationDocument>(DefaultProject.DefaultDocuments, serviceProvider).ToList();

            if (DefaultProject.LandingPage != null)
            {
                documentationIndex.DefaultProject.LandingPage = CreatRemoteDocument<TDocumentationDocument>(DefaultProject.LandingPage, serviceProvider);
            }

            documentationIndex.Projects = new List<IDocumentationProject>();
            foreach (var remoteProject in Projects)
            {
                TDocumentationProject project = new TDocumentationProject();
                project.Path = remoteProject.Path;
                project.KeyPath = remoteProject.KeyPath;
                project.Documents = CopyDocuments<TDocumentationDocument>(remoteProject.Documents, serviceProvider).ToList();
                project.DefaultDocuments = CopyDocuments<TDocumentationDocument>(remoteProject.DefaultDocuments, serviceProvider).ToList();

                if (remoteProject.LandingPage != null)
                    project.LandingPage = CreatRemoteDocument<TDocumentationDocument>(remoteProject.LandingPage, serviceProvider);

                documentationIndex.Projects.Add(project);
            }
            return documentationIndex;
        }

        private IEnumerable<IDocumentationDocument> CopyDocuments<TDocumentationDocument>(IEnumerable<RemoteDocumentationDocument> sourceDocumentationDocuments, IServiceProvider serviceProvider) where TDocumentationDocument : IDocumentationDocument, new()
        {
            List<IDocumentationDocument> finalDocumentationDocuments = new List<IDocumentationDocument>();
            foreach (var item in sourceDocumentationDocuments)
            {
                IDocumentationDocument doc = CreatRemoteDocument<TDocumentationDocument>(item, serviceProvider);
                finalDocumentationDocuments.Add(doc);
            }

            return finalDocumentationDocuments;
        }

        private IEnumerable<RemoteDocumentationDocument> CopyDocuments(IEnumerable<IDocumentationDocument> sourceDocumentationDocuments, string documentationRootPath)
        {
            List<RemoteDocumentationDocument> finalDocumentationDocuments = new List<RemoteDocumentationDocument>();
            foreach (var item in sourceDocumentationDocuments)
            {
                var doc = new RemoteDocumentationDocument
                {
                    Path = item.Path.Replace(documentationRootPath, "").Replace("\\", "/"),
                    LastUpdate = item.LastUpdate
                };
                if (item.SubDocumentsCount > 0)
                    doc.SubDocuments = CopyDocuments(item.SubDocuments, documentationRootPath).ToArray();

                finalDocumentationDocuments.Add(doc);
            }

            return finalDocumentationDocuments;
        }

        private IDocumentationDocument CreatRemoteDocument<TDocumentationDocument>(RemoteDocumentationDocument remoteDocumentationDocument, IServiceProvider serviceProvider) where TDocumentationDocument : IDocumentationDocument, new()
        {
            if (remoteDocumentationDocument.SubDocumentsCount > 0)
            {
                var doc = new GenericDocumentationDocument
                {
                    Path = remoteDocumentationDocument.Path,
                    LastUpdate = remoteDocumentationDocument.LastUpdate,
                    DocumentType = DocumentationDocumentType.Folder
                };

                doc.SubDocuments = CopyDocuments<TDocumentationDocument>(remoteDocumentationDocument.SubDocuments, serviceProvider).ToArray();
                return doc;
            }
            var docType = DocumentationHelper.GetDocumentationDocumentTypeFromExtension(Path.GetExtension(remoteDocumentationDocument.Path));

            switch (docType)
            {
                case DocumentationDocumentType.Markdown:
                    using (var scope = serviceProvider.CreateScope())
                    {
                        return new RemoteMarkdownDocument(serviceProvider)
                        {
                            Path = remoteDocumentationDocument.Path,
                            LastUpdate = remoteDocumentationDocument.LastUpdate
                        };
                    }
                case DocumentationDocumentType.Pdf:
                    return new RemotePdfDocument
                    {
                        Path = remoteDocumentationDocument.Path,
                        LastUpdate = remoteDocumentationDocument.LastUpdate
                    };

                case DocumentationDocumentType.Html:
                    return new RemoteHtmlDocument(serviceProvider)
                    {
                        Path = remoteDocumentationDocument.Path,
                        LastUpdate = remoteDocumentationDocument.LastUpdate
                    };

                case DocumentationDocumentType.Word:
                    return new RemoteWordDocument(serviceProvider)
                    {
                        Path = remoteDocumentationDocument.Path,
                        LastUpdate = remoteDocumentationDocument.LastUpdate
                    };

                case DocumentationDocumentType.Drawio:
                    return new RemoteDrawioDocument
                    {
                        Path = remoteDocumentationDocument.Path,
                        LastUpdate = remoteDocumentationDocument.LastUpdate
                    };

                case DocumentationDocumentType.Folder:
                default:
                    return new GenericDocumentationDocument
                    {
                        Path = remoteDocumentationDocument.Path,
                        LastUpdate = remoteDocumentationDocument.LastUpdate,
                        DocumentType = docType
                    };
            }
        }
    }
}