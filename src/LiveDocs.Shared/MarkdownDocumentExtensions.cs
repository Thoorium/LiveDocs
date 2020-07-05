using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LiveDocs.Shared
{
    public static class MarkdownDocumentExtensions
    {
        public static async Task<string> ToHtml(this MarkdownDocument markdownDocument, string urlBase = "")
        {
            using StringWriter stringWriter = new StringWriter();
            HtmlRenderer htmlRenderer = new HtmlRenderer(stringWriter);

            if (!string.IsNullOrWhiteSpace(urlBase))
            {
                // If the url is a file, we want to remove the query string.
                if (Path.GetExtension(urlBase) == "")
                    htmlRenderer.BaseUrl = new Uri(urlBase, UriKind.Absolute);
                else htmlRenderer.BaseUrl = new Uri(urlBase.Substring(0, urlBase.IndexOf("?") > 0 ? urlBase.IndexOf("?") : urlBase.Length), UriKind.Absolute);
            }
            
            htmlRenderer.Render(markdownDocument);
            await stringWriter.FlushAsync();

            return stringWriter.ToString();
        }
    }
}
