using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LiveDocs.Shared.Services;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;

namespace LiveDocs.Shared
{
    public static class MarkdownDocumentExtensions
    {
        public static async Task<string> ToHtml(this MarkdownDocument markdownDocument, IDocumentationProject documentationProject, string urlBase = "")
        {
            using StringWriter stringWriter = new StringWriter();
            HtmlRenderer htmlRenderer = new HtmlRenderer(stringWriter);

            if (!string.IsNullOrWhiteSpace(urlBase))
            {
                // If the url is a file, we want to remove the query string.
                if (Path.GetExtension(urlBase) == "")
                    htmlRenderer.BaseUrl = new Uri(urlBase, UriKind.Absolute);
                else htmlRenderer.BaseUrl = new Uri(UrlHelper.RemoveUrlQueryStrings(urlBase), UriKind.Absolute);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlTableRenderer());

            var linkRender = new LinkInlineRenderer
            {
                LinkRewriter = (originalUrl) => RewriteUrl(originalUrl, urlBase, documentationProject)
            };
            htmlRenderer.ObjectRenderers.ReplaceOrAdd<Markdig.Renderers.Html.Inlines.LinkInlineRenderer>(linkRender);

            htmlRenderer.Render(markdownDocument);
            await stringWriter.FlushAsync();

            return stringWriter.ToString();
        }

        /// <summary>
        /// Rewrite the url to match an existing document or identify an external uri.
        /// </summary>
        /// <param name="originalUrl">The original url on the link.</param>
        /// <param name="sourceUrl">Full url of the caller.</param>
        /// <param name="documentationProject">List of documents in the current project to match with.</param>
        /// <returns></returns>
        private static LinkInlineRenderer.LinkInlineRewrite RewriteUrl(string originalUrl, string sourceUrl, IDocumentationProject documentationProject)
        {
            var result = UrlHelper.RewriteUrl(originalUrl, sourceUrl, documentationProject);
            return new LinkInlineRenderer.LinkInlineRewrite { NewLink = result.NewUri, Target = result.Target };
        }
    }
}