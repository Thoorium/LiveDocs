using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LiveDocs.Shared;
using LiveDocs.Shared.Options;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents;
using LiveDocs.Shared.Services.Remote;
using LiveDocs.Shared.Services.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveDocs.Server.Services
{
    public class DocumentationService : IDocumentationService
    {
        private readonly IWebHostEnvironment _HostingEnvironment;
        private readonly ILogger<DocumentationService> _Logger;
        private readonly LiveDocsOptions _Options;
        private readonly SearchPipeline _SearchPipeline;

        public DocumentationService(ILogger<DocumentationService> logger, IOptions<LiveDocsOptions> options, IWebHostEnvironment hostingEnvironment, SearchPipeline searchPipeline)
        {
            _Logger = logger;
            _Options = options.Value;
            _HostingEnvironment = hostingEnvironment;
            _SearchPipeline = searchPipeline;
        }

        public IDocumentationIndex DocumentationIndex { get; set; }
        public ISearchIndex SearchIndex { get; set; }

        public Task<IDocumentationIndex> IndexFiles()
        {
            DirectoryInfo directoryInfo = _Options.GetDocumentationFolderAsAbsolute(_HostingEnvironment.ContentRootPath);

            IDocumentationIndex documentationIndex = new DocumentationIndex();

            IDocumentationProject documentationProject = new DocumentationProject(_Options.DefaultDocuments, _Options.LandingPageDocument, null);
            BuildDocumentationSubTree(directoryInfo, directoryInfo, documentationProject);

            foreach (var project in documentationProject.SubProjects)
            {
                documentationIndex.Projects.Add(project);
            }

            documentationIndex.DefaultProject = new DocumentationProject(_Options.DefaultDocuments, _Options.LandingPageDocument, "");
            documentationIndex.DefaultProject.Documents.AddRange(documentationProject.Documents.OrderBy(o => o.Name));

            return Task.FromResult(documentationIndex);
        }

        public async Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex)
        {
            int documentCountAfter = documentationIndex.DefaultProject.DocumentCount + documentationIndex.Projects.Sum(s => s.DocumentCount);
            if (DocumentationIndex == null)
                _Logger.LogInformation($"Initializing documentation index. {documentCountAfter} documents added.");
            else
            {
                int documentCountBefore = DocumentationIndex.DefaultProject.DocumentCount + DocumentationIndex.Projects.Sum(s => s.DocumentCount);
                _Logger.LogInformation($"Refreshing documentation index. Before {documentCountBefore}; After {documentCountAfter}.");
            }

            DocumentationIndex = documentationIndex;

            await SetProjectDefaultDocuments(DocumentationIndex.DefaultProject);

            foreach (var project in DocumentationIndex.Projects)
            {
                await SetProjectDefaultDocuments(project);
            }

            var documentationDirectoryInfo = _Options.GetDocumentationFolderAsAbsolute(_HostingEnvironment.ContentRootPath);
            RemoteConfiguration remoteConfiguration = new RemoteConfiguration
            {
                ApplicationName = _Options.ApplicationName,
                ShowDownloadOriginal = _Options.ShowDownloadOriginal,
                Documentation = new RemoteDocumentationIndex(documentationIndex, documentationDirectoryInfo.FullName)
            };
            var json = JsonSerializer.Serialize(remoteConfiguration, new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });
            var navDocumentPath = Path.Combine(documentationDirectoryInfo.FullName, "app.json");
            await File.WriteAllTextAsync(navDocumentPath, json);

            documentationIndex.DefaultProject.Documents.Add(new GenericDocumentationDocument
            {
                Path = navDocumentPath,
                LastUpdate = DateTime.Now
            });
        }

        public async Task RefreshSearchIndex(IDocumentationIndex documentationIndex)
        {
            var documentationDirectoryInfo = _Options.GetDocumentationFolderAsAbsolute(_HostingEnvironment.ContentRootPath);
            SearchIndex = new BasicSearchIndex(_SearchPipeline, documentationIndex);
            await SearchIndex.BuildIndex();

            var json = JsonSerializer.Serialize((BasicSearchIndex)SearchIndex, new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });
            var searchDocumentPath = Path.Combine(documentationDirectoryInfo.FullName, "search.json");
            await File.WriteAllTextAsync(searchDocumentPath, json);

            documentationIndex.DefaultProject.Documents.Add(new GenericDocumentationDocument
            {
                Path = searchDocumentPath,
                LastUpdate = DateTime.Now
            });
        }

        private DocumentationDocumentType BuildDocumentationSubTree(DirectoryInfo directoryInfo, DirectoryInfo topDirectoryInfo, IDocumentationProject project)
        {
            DocumentationDocumentType subTreeDocumentType = DocumentationDocumentType.Folder;
            project.Path = directoryInfo.FullName;

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                var docType = DocumentationHelper.GetDocumentationDocumentTypeFromExtension(Path.GetExtension(file.FullName));

                switch (docType)
                {
                    case DocumentationDocumentType.Markdown:
                        // The markdown pipeline isn't needed for the content extraction.
                        project.Documents.Add(new MarkdownDocument(null)
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Word:
                        project.Documents.Add(new WordDocument
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Pdf:
                    case DocumentationDocumentType.Html:
                    case DocumentationDocumentType.Folder:
                    case DocumentationDocumentType.Drawio:
                        project.Documents.Add(new DocumentationDocument
                        {
                            Path = file.FullName,
                            DocumentType = docType,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Project:
                        subTreeDocumentType = DocumentationDocumentType.Project;
                        break;

                    case DocumentationDocumentType.Unknown:
                        break;

                    default:
                        break;
                }
            }

            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                IDocumentationProject subProject = new DocumentationProject(_Options.DefaultDocuments, _Options.LandingPageDocument, project.KeyPath);
                var subDocumentType = BuildDocumentationSubTree(directory, topDirectoryInfo, subProject);

                if (subDocumentType == DocumentationDocumentType.Project)
                {
                    project.SubProjects.Add(subProject);
                } else
                {
                    var documentationDirectory = new DocumentationDocument
                    {
                        DocumentType = subDocumentType,
                        Path = directory.FullName,
                        LastUpdate = directory.LastWriteTimeUtc
                    };

                    if (subProject.Documents.Any())
                    {
                        documentationDirectory.SubDocuments = subProject.Documents.OrderBy(o => o.Name).ToArray();
                        project.Documents.Add(documentationDirectory);
                    }
                }
            }

            return subTreeDocumentType;
        }

        private async Task SetProjectDefaultDocuments(IDocumentationProject documentationProject)
        {
            documentationProject.DefaultDocuments.Clear();
            documentationProject.DefaultDocuments.AddRange(await documentationProject.GetDocumentationDefaultDocuments());
            documentationProject.LandingPage = await documentationProject.GetDocumentationLandingPageDocument();

            foreach (var subProject in documentationProject.SubProjects)
            {
                await SetProjectDefaultDocuments(subProject);
            }
        }
    }
}