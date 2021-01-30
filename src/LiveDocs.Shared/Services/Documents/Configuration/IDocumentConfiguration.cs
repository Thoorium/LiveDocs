namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public interface IDocumentConfiguration
    {
        public bool Enabled { get; set; }

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}