namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class DrawioDocumentConfiguration : IDocumentConfiguration
    {
        public bool Enabled { get; set; } = true;

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}