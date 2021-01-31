using System.Text.Json.Serialization;

namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class DrawioSvgDocumentConfiguration : IDocumentConfiguration
    {
        public enum BackgroundStyle
        {
            Light,
            Dark,
            Theme
        }

        public BackgroundStyle? DefaultBackground { get; set; }

        [JsonIgnore]
        public bool Enabled { get; set; } = true;

        public bool ShowDocumentNameAsTitle { get; set; }
    }
}