using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using LiveDocs.Shared.Services;

namespace LiveDocs.Shared
{
    public static class UrlHelper
    {
        public static string AddOrUpdateQueryString(string url, string name, string value)
        {
            var urlQueryRegex = Regex.Match(url, "\\?(.*?)($|#|\\?)");
            string urlQuery = "";

            if (urlQueryRegex.Success)
                urlQuery = urlQueryRegex.Groups[1].Value;

            string urlId = GetUrlId(url);

            if (!string.IsNullOrWhiteSpace(urlId))
            {
                urlId = "#" + urlId;
                url = url.Replace(urlId, "");
            }

            var queries = HttpUtility.ParseQueryString(urlQuery);

            queries[name] = value;

            string path = RemoveUrlQueryStrings(url);
            string query = "?";

            for (int i = 0; i < queries.Count; i++)
            {
                if (i > 0)
                    query += "&";
                string key = queries.Keys[i];
                query += $"{key}={queries[key]}";
            }

            return path + query + urlId;
        }

        public static string GetQueryString(string url)
        {
            var queryStringRegex = Regex.Match(url, "\\?(.*?)($|#|\\?)");
            string queryString = "";

            if (queryStringRegex.Success)
                queryString = queryStringRegex.Groups[1].Value;
            return queryString;
        }

        public static string GetUrlId(string url)
        {
            var urlIdRegex = Regex.Match(url, "#(.*?)($|#|\\?)");
            string urlId = "";

            if (urlIdRegex.Success)
                urlId = urlIdRegex.Groups[1].Value;
            return urlId;
        }

        public static string RemoveUrlId(string url)
        {
            return url.Substring(0, url.IndexOf("#") >= 0 ? url.IndexOf("#") : url.Length);
        }

        public static string RemoveUrlQueryStrings(string url)
        {
            return url.Substring(0, url.IndexOf("?") >= 0 ? url.IndexOf("?") : url.Length);
        }

        /// <summary>
        /// Rewrite the url to match an existing document or identify an external uri.
        /// </summary>
        /// <param name="originalUrl">The original url on the link.</param>
        /// <param name="sourceUrl">Full url of the caller.</param>
        /// <param name="documentationProject">List of documents in the current project to match with.</param>
        /// <returns></returns>
        public static LinkRewriteResult RewriteUrl(string originalUrl, string sourceUrl, IDocumentationProject documentationProject)
        {
            originalUrl = HttpUtility.UrlDecode(originalUrl);
            sourceUrl = HttpUtility.UrlDecode(sourceUrl);

            Uri urlBase = new Uri(UrlHelper.RemoveUrlQueryStrings(sourceUrl), UriKind.Absolute);

            // Inner page navigation or home navigation.
            if (originalUrl.StartsWith("#") || string.IsNullOrWhiteSpace(originalUrl) || originalUrl == "/")
                return new LinkRewriteResult(originalUrl);

            // If the url is a full one and the domain is different, open in a new tab.
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri originalUri) && !string.IsNullOrWhiteSpace(originalUri.Host) && !originalUri.Host.Equals(urlBase.Host, StringComparison.InvariantCultureIgnoreCase))
                return new LinkRewriteResult(originalUrl) { Target = "_blank" };

            if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out Uri sourceUri))
                originalUri = new Uri(sourceUri, originalUrl);

            // Get the host url to clear the original url.
            var hostUrl = urlBase.AbsoluteUri.Replace(urlBase.AbsolutePath, "");

            // Keep a copy of the url id, if there's one.
            var urlId = UrlHelper.GetUrlId(originalUrl);
            var urlQueryString = UrlHelper.GetQueryString(originalUrl);

            // Extract the url parts. Skip the first entry as it is always null.
            var urlParts = UrlHelper.RemoveUrlId(HttpUtility.UrlDecode(originalUri?.AbsolutePath) ?? originalUrl).Replace(hostUrl + documentationProject.KeyPath, "")
                .Replace($"#{urlId}", "") //Remove the ID part
                .Replace($"?{urlQueryString}", "") // Remove the query string part
                .Split("/") // Split into parts
                .Skip(1) // Remove the first entry, it is always null
                .ToArray();

            if (urlParts.FirstOrDefault()?.Equals(documentationProject.Key, StringComparison.InvariantCultureIgnoreCase) ?? false)
                urlParts[0] = null;

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
                return new LinkRewriteResult(originalUrl) { Target = "_blank" };

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
            return new LinkRewriteResult(finalUrl);
        }

        /// <summary>
        /// Rewrite a text string to be a valid browsable url.
        /// </summary>
        /// <param name="stringUrl"></param>
        /// <returns></returns>
        public static string Urilize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Markdig.Helpers.LinkHelper.Urilize(text, true)?.Replace(".", "");
        }
    }
}