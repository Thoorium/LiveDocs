using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Search;

namespace LiveDocs.WebApp.Services
{
    public class SearchResult : ISearchResult
    {
        public IDocumentationDocument Document { get; set; }
        public string KeyPath { get; set; }
    }
}
