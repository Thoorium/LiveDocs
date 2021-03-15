using System;
using System.Collections.Generic;
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

        public Task<List<DocumentTreeItem>> GetDocumentTree()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetTitle()
        {
            var cacheResult = await TryCache();
            if (cacheResult != DocumentCacheResult.Success)
                return "";

            return await base.GetTitle();
        }

        public async Task<DocumentCacheResult> TryCache()
        {
            if (content != null)
                return DocumentCacheResult.Success;

            while (readingCache)
                await Task.Delay(5);

            if (content != null)
                return DocumentCacheResult.Success;

            readingCache = true;

            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    content = await httpClient.GetStringAsync(Path);
                }
            } catch (HttpRequestException)
            {
                return DocumentCacheResult.Offline;
            } catch
            {
                return DocumentCacheResult.Error;
            } finally
            {
                readingCache = false;
            }
            return DocumentCacheResult.Success;
        }

        public async Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            var cacheResult = await TryCache();
            string htmlString = "";
            if (cacheResult == DocumentCacheResult.Success)
                htmlString = await ToHtml(documentationProject, baseUri);

            return (cacheResult == DocumentCacheResult.Success, htmlString);
        }
    }
}