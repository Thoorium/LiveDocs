using LiveDocs.Shared.Services.Documents.Configuration;
using LiveDocs.Shared.Services.Search.Configuration;

namespace LiveDocs.Shared.Options
{
    public interface ILiveDocsOptions
    {
        public string ApplicationName { get; set; }
        public DocumentConfiguration Documents { get; set; }
        public NavigationConfiguration Navigation { get; set; }
        public SearchConfiguration Search { get; set; }
    }
}