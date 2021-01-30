namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class PdfDocumentConfiguration : IDocumentConfiguration
    {
        public bool Enabled { get; set; } = true;

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}