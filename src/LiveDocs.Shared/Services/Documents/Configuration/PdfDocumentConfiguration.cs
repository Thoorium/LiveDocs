using System.Text.Json.Serialization;

namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class PdfDocumentConfiguration : IDocumentConfiguration
    {
        [JsonIgnore]
        public bool Enabled { get; set; } = true;

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}