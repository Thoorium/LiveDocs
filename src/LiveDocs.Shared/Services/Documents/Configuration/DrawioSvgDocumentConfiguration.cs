namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class DrawioSvgDocumentConfiguration : IDocumentConfiguration
    {
        public bool Enabled { get; set; } = true;

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}