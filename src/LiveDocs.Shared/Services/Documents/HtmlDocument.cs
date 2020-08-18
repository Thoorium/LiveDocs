using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public class HtmlDocument : IDocumentationDocument
    {
        private static Regex MatchHeaderOneRegex = new Regex("<[hH]1.*>(.*?)<\\/[hH]1>");
        // TODO: Replace with content cache when it's done.
        private string content = null;

        public virtual string Content
        {
            get
            {
                if (content == null)
                    content = File.ReadAllText(Path);
                return content;
            }
        }

        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Html;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;
        public Task<string> GetTitle()
        {
            var match = MatchHeaderOneRegex.Match(Content);

            if (match.Success)
                return Task.FromResult(match.Groups[1].Value);

            return Task.FromResult(Name);
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            return Task.FromResult(Content);
        }
    }
}