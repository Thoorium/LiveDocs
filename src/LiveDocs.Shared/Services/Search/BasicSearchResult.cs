namespace LiveDocs.Shared.Services.Search
{
    public class BasicSearchResult : ISearchResult
    {
        public IDocumentationDocument Document { get; set; }

        // TODO: HitCount no longer represents hit count.
        public int HitCount { get; set; } = 0;

        public string KeyPath { get; set; }
    }
}