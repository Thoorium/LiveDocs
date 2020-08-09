namespace LiveDocs.Shared.Services.Search
{
    public interface ISearchResult
    {
        public IDocumentationDocument Document { get; set; }
        public string KeyPath { get; set; }
    }
}
