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
        public async Task Cache()
        {
            if (markdown != null)
                return;

            while (readingCache)
                await Task.Delay(5);

            if (markdown != null)
                return;

            readingCache = true;

            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    var markdownPipeline = scope.ServiceProvider.GetRequiredService<MarkdownPipeline>();
                    markdown = Markdig.Markdown.Parse(await httpClient.GetStringAsync(Path), markdownPipeline);
                }
            } finally
            {
                readingCache = false;
            }
        }

        public async Task<string> GetTitle(HttpClient httpClient)
        {
            await Cache();
            return await base.GetTitle();
        }

        public async Task<string> ToHtml(HttpClient httpClient, IDocumentationProject documentationProject, string baseUri = "")
        {
            await Cache();
            return await ToHtml(documentationProject, baseUri);
        }
    }
}