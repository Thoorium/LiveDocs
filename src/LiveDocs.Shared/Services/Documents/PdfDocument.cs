using System;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public class PdfDocument : IDocumentationDocument
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Pdf;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string FileName => System.IO.Path.GetFileName(Path);
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            string path = UrlHelper.RemoveUrlId(baseUri);
            path = UrlHelper.RemoveUrlQueryStrings(path);

            string pdfPath = $"{path}.pdf";
            return Task.FromResult($"<object class=\"pdf\" data=\"{pdfPath}\" type =\"application/pdf\" width =\"100%\" height=\"100 %\">This browser does not support embedded pdf. Please download the pdf to view it: <a href=\"{pdfPath}\" target=\"_blank\" rel=\"noopener noreferrer\">{Name}.pdf</a>.</object>");
        }
    }
}
