using System.Text.Json.Serialization;

namespace LiveDocs.Shared.Services.Documents.Configuration
{
    public class DocumentConfiguration
    {
        public DrawioDocumentConfiguration Drawio { get; set; }

        public DrawioSvgDocumentConfiguration DrawioSvg { get; set; }

        public HtmlDocumentConfiguration Html { get; set; }

        public MarkdownDocumentConfiguration Markdown { get; set; }

        public PdfDocumentConfiguration Pdf { get; set; }

        [JsonIgnore] // TODO: Enable for 1.x.
        public string[] Priority { get; set; }

        /// <summary>
        /// Show the "Download Original" download link above documents.
        /// </summary>
        public bool ShowDownloadOriginal { get; set; }

        public WordDocumentConfiguration Word { get; set; }
    }
}