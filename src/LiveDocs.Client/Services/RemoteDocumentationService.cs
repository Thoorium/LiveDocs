using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LiveDocs.Client.Services.Documents;
using LiveDocs.Shared.Options;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents.Configuration;
using LiveDocs.Shared.Services.Search;
using LiveDocs.Shared.Services.Search.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Client.Services
{
    public class RemoteDocumentationService : IDocumentationService
    {
        private readonly IServiceProvider _Services;

        public RemoteDocumentationService(IServiceProvider services)
        {
            _Services = services;
        }

        public IDocumentationIndex DocumentationIndex { get; set; }
        public ISearchIndex SearchIndex { get; set; }

        public async Task<IDocumentationIndex> IndexFiles()
        {
            using (var scope = _Services.CreateScope())
            {
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                var remoteConfiguration = await httpClient.GetFromJsonAsync<RemoteLiveDocsOptions>("app.json");

                var configuration = scope.ServiceProvider.GetRequiredService<RemoteLiveDocsOptions>();
                Console.WriteLine(remoteConfiguration.ApplicationName);
                configuration.ApplicationName = remoteConfiguration.ApplicationName;

                if (string.IsNullOrWhiteSpace(configuration.ApplicationName))
                    configuration.ApplicationName = "LiveDocs";

                configuration.Documents = remoteConfiguration.Documents ?? new DocumentConfiguration();
                configuration.Search = remoteConfiguration.Search ?? new SearchConfiguration();
                configuration.Navigation = remoteConfiguration.Navigation ?? new NavigationConfiguration();

                return remoteConfiguration.Documentation.ToDocumentationIndex<DocumentationIndex, DocumentationProject, DocumentationDocument>(_Services);
            }
        }

        public Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex)
        {
            DocumentationIndex = documentationIndex;
            return Task.CompletedTask;
        }

        public async Task RefreshSearchIndex(IDocumentationIndex documentationIndex)
        {
            if (SearchIndex != null)
                return;

            using (var scope = _Services.CreateScope())
            {
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                var index = await httpClient.GetFromJsonAsync<BasicSearchIndex>("search.json");
                var searchPipeline = scope.ServiceProvider.GetRequiredService<SearchPipeline>();
                var options = scope.ServiceProvider.GetRequiredService<RemoteLiveDocsOptions>();
                index.Setup(searchPipeline, documentationIndex, options);
                SearchIndex = index;
            }
        }
    }
}