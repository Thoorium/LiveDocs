using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LiveDocs.Client.Services.Documents;
using LiveDocs.Server.Services.Remote;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Search;
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
                var index = await httpClient.GetFromJsonAsync<RemoteDocumentationIndex>("nav.json");
                return index.ToDocumentationIndex<DocumentationIndex, DocumentationProject, DocumentationDocument>(_Services);
            }
        }

        public Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex)
        {
            DocumentationIndex = documentationIndex;
            return Task.CompletedTask;
        }

        public Task RefreshSearchIndex(IDocumentationIndex documentationIndex)
        {
            throw new NotImplementedException();
        }
    }
}