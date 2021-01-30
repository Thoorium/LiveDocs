using System;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public class DrawioSvgDocument : IDocumentationDocument
    {
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.DrawioSvg;
        public string FileName => DocumentationHelper.GetDocumentNameWithoutExtension(System.IO.Path.GetFileName(Path));
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => DocumentationHelper.GetDocumentNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            string path = UrlHelper.RemoveUrlId(baseUri);
            path = UrlHelper.RemoveUrlQueryStrings(path);

            string drawioSvgPath = $"{path}.drawio.svg";
            return Task.FromResult($"<img src=\"{drawioSvgPath}\" class=\"img-fluid mx-auto d-block\" />");
        }
    }
}