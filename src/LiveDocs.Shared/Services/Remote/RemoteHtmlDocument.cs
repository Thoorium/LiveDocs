using System;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteHtmlDocument : HtmlDocument, IRemoteDocumentationDocument
    {
        private readonly IServiceProvider _Services;
        private string content;
        private bool readingCache = false;
        public RemoteHtmlDocument(IServiceProvider serviceProvider)
        {
            _Services = serviceProvider;
        }

        public override string Content => content;

        public async Task<string> GetTitle()
        {
            var cacheResult = await TryCache();
            if (!cacheResult)
                return "";

            return await base.GetTitle();
        }

        public async Task<bool> TryCache()
        {
            if (content != null)
                return true;

            while (readingCache)
                await Task.Delay(5);

            if (content != null)
                return true;

            readingCache = true;

            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    content = await httpClient.GetStringAsync(Path);
                }
            } catch
            {
                return false;
            } finally
            {
                readingCache = false;
            }
            return true;
        }

        public async Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            var cacheResult = await TryCache();
            string htmlString = "";
            if (cacheResult)
                htmlString = await ToHtml(documentationProject, baseUri);

            return (cacheResult, htmlString);
        }
    }
}