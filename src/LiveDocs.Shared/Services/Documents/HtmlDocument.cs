using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public class HtmlDocument : IDocumentationDocument
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Html;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string FileName => System.IO.Path.GetFileName(Path);
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        // TODO: Replace with content cache when it's done.
        string content = null;

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            if(content == null)
                content = await File.ReadAllTextAsync(Path);
            return content;
        }

        private static Regex MatchHeaderOneRegex = new Regex("<[hH]1.*>(.*?)<\\/[hH]1>");

        public Task<string> GetTitle()
        {
            var match = MatchHeaderOneRegex.Match(content);

            if (match.Success)
                return Task.FromResult(match.Groups[1].Value);

            return Task.FromResult(Name);
        }
    }
}
