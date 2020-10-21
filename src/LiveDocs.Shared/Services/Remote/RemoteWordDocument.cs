using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;
using Microsoft.Extensions.DependencyInjection;
using Thoorium.WordLib.Elements;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteWordDocument : WordDocument, IRemoteDocumentationDocument
    {
        private readonly IServiceProvider _Services;
        private Thoorium.WordLib.WordDocument document;
        private List<DocumentTreeItem> documentTree = new List<DocumentTreeItem>();
        private bool readingCache;

        public RemoteWordDocument(IServiceProvider serviceProvider)
        {
            _Services = serviceProvider;
        }

        public override Thoorium.WordLib.WordDocument Document => document;

        public async Task<List<DocumentTreeItem>> GetDocumentTree()
        {
            if (documentTree.Count > 0)
                return documentTree;

            var cacheResult = await TryCache();
            if (!cacheResult)
                return documentTree;

            foreach (var element in Document.Elements)
            {
                if (element is HeadingElement headingElement)
                {
                    var linkFound = headingElement.HtmlAttributes.TryGetValue("id", out string link);
                    documentTree.Add(new DocumentTreeItem
                    {
                        HeaderText = headingElement.Value,
                        HeaderLevel = headingElement.Level,
                        HeaderLink = linkFound ? link : ""
                    });
                }
            }

            return documentTree;
        }

        public async Task<bool> TryCache()
        {
            if (Document != null)
                return true;

            while (readingCache)
                await Task.Delay(5);

            if (Document != null)
                return true;

            readingCache = true;
            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    document = await Thoorium.WordLib.WordDocument.LoadAsync(await httpClient.GetStreamAsync(Path));
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