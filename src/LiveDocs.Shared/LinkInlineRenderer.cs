// SOURCE: https://github.com/lunet-io/markdig/blob/4f7ef613033c410ab85fe222ad74b1d4304abfc0/src/Markdig/Renderers/Html/Inlines/LinkInlineRenderer.cs
// AUTHOR:  lunet-io/markdig 
// DATE: 2020-08-06

using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using System;

namespace LiveDocs.Shared
{
    public class LinkInlineRenderer : HtmlObjectRenderer<LinkInline>
    {
        public class LinkInlineRewrite
        {
            public string NewLink { get; set; }
            public string Target { get; set; }
        }
        /// <summary>
        /// Gets or sets the literal string in property rel for links
        /// </summary>
        public string Rel { get; set; }

        /// <summary>
        /// Function to rewrite urls and set the target.
        /// </summary>
        public Func<string, LinkInlineRewrite> LinkRewriter { get; set; }

        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            var linkRewriteResult = LinkRewriter?.Invoke(link.Url);

            if (renderer.EnableHtmlForInline)
            {
                renderer.Write(link.IsImage ? "<img src=\"" : "<a href=\"");
                renderer.WriteEscapeUrl(linkRewriteResult?.NewLink ?? link.Url);
                renderer.Write("\"");
                renderer.WriteAttributes(link);
            }
            if (link.IsImage)
            {
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write(" alt=\"");
                }
                var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
                renderer.EnableHtmlForInline = false;
                renderer.WriteChildren(link);
                renderer.EnableHtmlForInline = wasEnableHtmlForInline;
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write("\"");
                }
            }

            if (renderer.EnableHtmlForInline && !string.IsNullOrEmpty(link.Title))
            {
                renderer.Write(" title=\"");
                renderer.WriteEscape(link.Title);
                renderer.Write("\"");
            }

            if(renderer.EnableHtmlForInline && !link.IsImage && !string.IsNullOrWhiteSpace(linkRewriteResult?.Target))
            {
                renderer.Write($" target=\"{linkRewriteResult.Target}\"");
                if (string.IsNullOrWhiteSpace(Rel))
                {
                    renderer.Write($" rel=\"noopener noreferrer\"");
                }
            }

            if (link.IsImage)
            {
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write(" />");
                }
            } else
            {
                if (renderer.EnableHtmlForInline)
                {
                    if (!string.IsNullOrWhiteSpace(Rel))
                    {
                        renderer.Write($" rel=\"{Rel}\"");
                    }
                    renderer.Write(">");
                }
                renderer.WriteChildren(link);
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write("</a>");
                }
            }
        }
    }
}
