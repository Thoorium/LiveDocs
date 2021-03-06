﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<DocumentTreeItem> documentTree = new List<DocumentTreeItem>();
        private Markdig.Syntax.MarkdownDocument markdown;
        private bool readingCache = false;

        public RemoteMarkdownDocument(IServiceProvider serviceProvider) : base(serviceProvider.GetRequiredService<MarkdownPipeline>())
        {
            _Services = serviceProvider;
        }

        public override Markdig.Syntax.MarkdownDocument Markdown => markdown;

        public Task<List<DocumentTreeItem>> GetDocumentTree()
        {
            if (documentTree.Count > 0)
                return Task.FromResult(documentTree);

            foreach (Markdig.Syntax.HeadingBlock header in Markdown.Where(w => w is Markdig.Syntax.HeadingBlock))
            {
                string headerText = ExtractLiterals(header.Inline);
                string headerLink = UrlHelper.Urilize(headerText);

                string uniqueHeaderLink = headerLink;
                int numPad = 1;
                while (documentTree.Any(a => a.HeaderLink == uniqueHeaderLink))
                    uniqueHeaderLink += $"-{numPad++}";

                documentTree.Add(new DocumentTreeItem
                {
                    HeaderText = headerText,
                    HeaderLink = uniqueHeaderLink,
                    HeaderLevel = header.Level
                });
            }

            return Task.FromResult(documentTree);
        }

        public async Task<DocumentCacheResult> TryCache()
        {
            if (markdown != null)
                return DocumentCacheResult.Success;

            while (readingCache)
                await Task.Delay(5);

            if (markdown != null)
                return DocumentCacheResult.Success;

            readingCache = true;
            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    var markdownPipeline = scope.ServiceProvider.GetRequiredService<MarkdownPipeline>();
                    markdown = Markdig.Markdown.Parse(await httpClient.GetStringAsync(Path), markdownPipeline);
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

        private string ExtractLiterals(Markdig.Syntax.Inlines.ContainerInline parentInline, string text = "")
        {
            foreach (var inline in parentInline)
            {
                text += inline switch
                {
                    Markdig.Syntax.Inlines.LiteralInline literal =>
                        literal.ToString(),
                    Markdig.Syntax.Inlines.CodeInline code =>
                        code.Content,
                    Markdig.Syntax.Inlines.HtmlInline htmlInline =>
                       "", // The text between html tags is a literal.
                    Markdig.Syntax.Inlines.ContainerInline container =>
                        ExtractLiterals(container),
                    _ => inline.ToString()
                };
            }
            return text;
        }
    }
}