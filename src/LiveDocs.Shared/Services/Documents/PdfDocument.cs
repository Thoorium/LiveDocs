using System;
using System.Collections.Generic;
using System.Text;
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

        public IDocumentationDocument[] SubDocuments { get; set; } = null;

        public Task<string> ToHtml(string baseUri)
        {
            string path = baseUri.Substring(0, baseUri.IndexOf("#") > 0 ? baseUri.IndexOf("#") : baseUri.Length);
            path = path.Substring(0, path.IndexOf("?") > 0 ? path.IndexOf("?") : path.Length);

            // TODO: Figure how to serve the file using the key instead of the name.
            string pdfPath = $"{path.Replace(Key, Name)}.pdf";
            return Task.FromResult($"<object class=\"pdf\" data=\"{pdfPath}\" type =\"application/pdf\" width =\"100%\" height=\"100 %\">This browser does not support embedded pdf. Please download the pdf to view it: <a href=\"{pdfPath}\" target=\"_blank\" rel=\"noopener noreferrer\">{Name}.pdf</a>.</object>");
        }
    }
}
