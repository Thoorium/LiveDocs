using System;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;
using Markdig;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteMarkdownDocument : MarkdownDocument, IRemoteDocumentationDocument
    {
        private readonly IServiceProvider _Services;
        private Markdig.Syntax.MarkdownDocument markdown;
        private bool readingCache = false;
        public RemoteMarkdownDocument(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<MarkdownPipeline>())
        {
            _Services = serviceProvider;
        }

        public override Markdig.Syntax.MarkdownDocument Markdown => markdown;

        public async Task<bool> TryCache()
        {
            if (markdown != null)
                return true;

            while (readingCache)
                await Task.Delay(5);

            if (markdown != null)
                return true;

            readingCache = true;
            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    var markdownPipeline = scope.ServiceProvider.GetRequiredService<MarkdownPipeline>();
                    markdown = Markdig.Markdown.Parse(await httpClient.GetStringAsync(Path), markdownPipeline);
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
            if(cacheResult)
                htmlString = await ToHtml(documentationProject, baseUri);

            return (cacheResult, htmlString);
        }
    }
}