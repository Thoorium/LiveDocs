using LiveDocs.Shared.Services;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LiveDocs.Shared
{
    public static class MarkdownDocumentExtensions
    {
        public static async Task<string> ToHtml(this MarkdownDocument markdownDocument, IDocumentationProject documentationProject, string urlBase = "")
        {
            using StringWriter stringWriter = new StringWriter();
            HtmlRenderer htmlRenderer = new HtmlRenderer(stringWriter);

            htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlTableRenderer());

            if (!string.IsNullOrWhiteSpace(urlBase))
            {
                // If the url is a file, we want to remove the query string.
                if (Path.GetExtension(urlBase) == "")
                    htmlRenderer.BaseUrl = new Uri(urlBase, UriKind.Absolute);
                else htmlRenderer.BaseUrl = new Uri(UrlHelper.RemoveUrlQueryStrings(urlBase), UriKind.Absolute);
            }

            htmlRenderer.LinkRewriter = (originalUrl) => RewriteUrl(originalUrl, htmlRenderer.BaseUrl, documentationProject);

            htmlRenderer.Render(markdownDocument);
            await stringWriter.FlushAsync();

            return stringWriter.ToString();
        }

        private static string RewriteUrl(string originalUrl, Uri urlBase, IDocumentationProject documentationProject)
        {
            // Get the host url to clear the original url.
            var hostUrl = urlBase.AbsoluteUri.Replace(urlBase.AbsolutePath, "");

            // Keep a copy of the url id, if there's one.
            var urlId = UrlHelper.GetUrlId(originalUrl);

            // Extract the url parts.
            var urlParts = UrlHelper.RemoveUrlId(originalUrl).Replace(hostUrl + documentationProject.KeyPath, "").Replace("%20", " ").Split("/");

            // Normalize all the elements up to the file name to enable search by key.
            for (int i = 0; i < urlParts.Length - 1; i++)
            {
                urlParts[i] = Markdig.Helpers.LinkHelper.Urilize(urlParts[i], allowOnlyAscii: true);
            }

            // Try to find a corresponding document by the document file name.
            var document = documentationProject.GetDocumentByFileName(urlParts)?.Result;

            // No matching document found, return the original url.
            if (document == null)
                return originalUrl;

            // Replace the file name with the matching document key.
            urlParts[^1] = document.Key;

            // Merge back the url parts into an url.
            var finalUrl = documentationProject.KeyPath + string.Join("/", urlParts);

            // Put back the id if there was one.
            if (!string.IsNullOrWhiteSpace(urlId))
                finalUrl += $"#{urlId}";
            return finalUrl;
        }
    }
}
