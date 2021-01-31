using LiveDocs.Shared.Options;
using LiveDocs.Shared.Services.Documents.Configuration;
using LiveDocs.Shared.Services.Search.Configuration;

namespace LiveDocs.Shared.Tests
{
    public class TestLiveDocsOptions : ILiveDocsOptions
    {
        public string ApplicationName { get; set; }
        public DocumentConfiguration Documents { get; set; }
        public SearchConfiguration Search { get; set; }
    }
}