using System.Text.RegularExpressions;
using System.Web;

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

        public static string GetUrlId(string url)
        {
            var urlIdRegex = Regex.Match(url, "#(.*?)($|#|\\?)");
            string urlId = "";

            if (urlIdRegex.Success)
                urlId = urlIdRegex.Groups[1].Value;
            return urlId;
        }

        public static string GetQueryString(string url)
        {
            var queryStringRegex = Regex.Match(url, "\\?(.*?)($|#|\\?)");
            string queryString = "";

            if (queryStringRegex.Success)
                queryString = queryStringRegex.Groups[1].Value;
            return queryString;
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
        /// Rewrite a text string to be a valid browsable url.
        /// </summary>
        /// <param name="stringUrl"></param>
        /// <returns></returns>
        public static string Urilize(string text)
        {
            return Markdig.Helpers.LinkHelper.Urilize(text, true)?.Replace(".", "-");
        }
    }
}
