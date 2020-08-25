using System;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace LiveDocs.Shared.Services.Documents
{
    public class MarkdownDocument : IDocumentationDocument, ISearchableDocument
    {
        protected readonly MarkdownPipeline MarkdownPipeline;
        public MarkdownDocument(MarkdownPipeline markdownPipeline)
        {
            MarkdownPipeline = markdownPipeline;
        }

        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Markdown;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public virtual Markdig.Syntax.MarkdownDocument Markdown => Markdig.Markdown.Parse(System.IO.File.ReadAllText(Path), MarkdownPipeline);
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;
        public Task<string> GetContent()
        {
            return Task.FromResult(string.Join(" ", Markdown.Descendants<LiteralInline>().Select(s => s.ToString())));
        }

        public Task<string> GetTitle()
        {
            HeadingBlock header = Markdown.FirstOrDefault(f => f is HeadingBlock) as HeadingBlock;

            if (header != null)
                return Task.FromResult(string.Join("", header.Inline));
            return Task.FromResult(Name);
        }

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            return await Markdown.ToHtml(documentationProject, baseUri);
        }
    }
}