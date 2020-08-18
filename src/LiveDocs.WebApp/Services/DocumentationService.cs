using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Shared;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents;
using LiveDocs.Shared.Services.Search;
using LiveDocs.WebApp.Options;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationService : IDocumentationService
    {
        private readonly IWebHostEnvironment _HostingEnvironment;
        private readonly ILogger<DocumentationService> _Logger;
        private readonly MarkdownPipeline _MarkdownPipeline;
        private readonly LiveDocsOptions _Options;
        public DocumentationService(ILogger<DocumentationService> logger, IOptions<LiveDocsOptions> options, IWebHostEnvironment hostingEnvironment, MarkdownPipeline markdownPipeline)
        {
            _Logger = logger;
            _Options = options.Value;
            _HostingEnvironment = hostingEnvironment;
            _MarkdownPipeline = markdownPipeline;
        }

        public IDocumentationIndex DocumentationIndex { get; set; }
        public ISearchIndex SearchIndex { get; set; }
        public Task<IDocumentationIndex> IndexFiles()
        {
            DirectoryInfo directoryInfo = _Options.GetDocumentationFolderAsAbsolute(_HostingEnvironment.ContentRootPath);

            IDocumentationIndex documentationIndex = new DocumentationIndex();

            IDocumentationProject documentationProject = new DocumentationProject(_Options, null);
            BuildDocumentationSubTree(directoryInfo, directoryInfo, documentationProject);

            foreach (var project in documentationProject.SubProjects)
            {
                documentationIndex.Projects.Add(project);
            }

            documentationIndex.DefaultProject = new DocumentationProject(_Options, "");
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
        }

        public async Task RefreshSearchIndex(IDocumentationIndex documentationIndex)
        {
            SearchIndex = new LuceneSearchIndex(documentationIndex);
            await SearchIndex.BuildIndex();
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
                        project.Documents.Add(new MarkdownDocument(_MarkdownPipeline)
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Pdf:
                        project.Documents.Add(new PdfDocument
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Html:
                        project.Documents.Add(new HtmlDocument
                        {
                            Path = file.FullName,
                            LastUpdate = file.LastWriteTimeUtc
                        });
                        break;

                    case DocumentationDocumentType.Word:
                    case DocumentationDocumentType.Folder:
                        project.Documents.Add(new GenericDocumentationDocument
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
                IDocumentationProject subProject = new DocumentationProject(_Options, project.KeyPath);
                var subDocumentType = BuildDocumentationSubTree(directory, topDirectoryInfo, subProject);

                if (subDocumentType == DocumentationDocumentType.Project)
                {
                    project.SubProjects.Add(subProject);
                } else
                {
                    var documentationDirectory = new GenericDocumentationDocument
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