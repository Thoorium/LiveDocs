using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public IDocumentationDocument[] SubDocuments { get; set; } = null;

        // TODO: Replace with content cache when it's done.
        string content = null;

        public async Task<string> ToHtml(string baseUri = "")
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
