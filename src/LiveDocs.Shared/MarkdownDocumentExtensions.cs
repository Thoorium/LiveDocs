using System;
using System.IO;
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
                LinkRewriter = (originalUrl) => RewriteUrl(originalUrl, htmlRenderer.BaseUrl, urlBase, documentationProject)
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
        /// <param name="urlBase">Domain base of the caller.</param>
        /// <param name="sourceUrl">Full url of the caller.</param>
        /// <param name="documentationProject">List of documents in the current project to match with.</param>
        /// <returns></returns>
        private static LinkInlineRenderer.LinkInlineRewrite RewriteUrl(string originalUrl, Uri urlBase, string sourceUrl, IDocumentationProject documentationProject)
        {
            originalUrl = HttpUtility.UrlDecode(originalUrl);
            sourceUrl = HttpUtility.UrlDecode(sourceUrl);

            // Inner page navigation or home navigation.
            if (originalUrl.StartsWith("#") || string.IsNullOrWhiteSpace(originalUrl) || originalUrl == "/")
                return new LinkInlineRenderer.LinkInlineRewrite { NewLink = originalUrl };

            // If the url is a full one and the domain is different, open in a new tab.
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri originalUri) && !originalUri.Host.Equals(urlBase.Host, StringComparison.InvariantCultureIgnoreCase))
                return new LinkInlineRenderer.LinkInlineRewrite { NewLink = originalUrl, Target = "_blank" };

            if (originalUrl.StartsWith("..") && Uri.TryCreate(sourceUrl, UriKind.Absolute, out Uri sourceUri))
                originalUri = new Uri(sourceUri, originalUrl);

            // Get the host url to clear the original url.
            var hostUrl = urlBase.AbsoluteUri.Replace(urlBase.AbsolutePath, "");

            // Keep a copy of the url id, if there's one.
            var urlId = UrlHelper.GetUrlId(originalUrl);
            var urlQueryString = UrlHelper.GetQueryString(originalUrl);

            // Extract the url parts.
            var urlParts = UrlHelper.RemoveUrlId(originalUri?.AbsolutePath ?? originalUrl).Replace(hostUrl + documentationProject.KeyPath, "").Replace($"#{urlId}", "").Replace($"?{urlQueryString}", "").Split("/");

            // Normalize all the elements up to the file name to enable search by key.
            for (int i = 0; i < urlParts.Length - 1; i++)
            {
                urlParts[i] = UrlHelper.Urilize(urlParts[i]);
            }

            // Try to find a corresponding document by the document file name.
            var document = documentationProject.GetDocumentByFileName(urlParts)?.Result;

            if (document == null)
                document = documentationProject.GetDocumentFor(urlParts)?.Result;

            // No matching document found, return the original url to open in a new url since this might be an external document.
            if (document == null)
                return new LinkInlineRenderer.LinkInlineRewrite { NewLink = originalUrl, Target = "_blank" };

            // Replace the file name with the matching document key.
            urlParts[^1] = document.Key;

            // Merge back the url parts into an url.
            var finalUrl = $"/{string.Join("/", urlParts)}".Replace("//", "/");

            if (!string.IsNullOrWhiteSpace(documentationProject.KeyPath))
                finalUrl = $"/{documentationProject.KeyPath}{finalUrl}";

            if (!string.IsNullOrWhiteSpace(urlQueryString))
                finalUrl += $"?{urlQueryString}";

            // Put back the id if there was one.
            if (!string.IsNullOrWhiteSpace(urlId))
                finalUrl += $"#{urlId}";
            return new LinkInlineRenderer.LinkInlineRewrite { NewLink = finalUrl };
        }
    }
}