using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LiveDocs.Shared.Services.Documents
{
    public class DrawioDocument : IDocumentationDocument
    {
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Drawio;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            string path = UrlHelper.RemoveUrlId(baseUri);
            path = UrlHelper.RemoveUrlQueryStrings(path);

            string drawioPath = $"{path}.drawio";
            return Task.FromResult($"<div id=\"mxgraph\" data-mxgraph=\"{{&quot;highlight&quot;:&quot;#0000ff&quot;,&quot;nav&quot;:true,&quot;resize&quot;:true,&quot;toolbar&quot;:&quot;zoom layers lightbox&quot;,&quot;edit&quot;:&quot;_blank&quot;,&quot;url&quot;:&quot;{drawioPath}&quot;}}\"></div>");
        }
    }
}