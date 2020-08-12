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
        public async Task Cache()
        {
            if (content != null)
                return;

            while (readingCache)
                await Task.Delay(5);

            if (content != null)
                return;

            readingCache = true;

            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    content = await httpClient.GetStringAsync(Path);
                }
            } finally
            {
                readingCache = false;
            }
        }

        public async Task<string> GetTitle()
        {
            await Cache();
            return await base.GetTitle();
        }

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            await Cache();
            return await base.ToHtml(documentationProject, baseUri);
        }
    }
}