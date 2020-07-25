using Markdig;
using Markdig.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public class MarkdownDocument : IDocumentationDocument
    {
        public string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        public string Path { get; set; }
        public DateTime LastUpdate { get; set; }
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Markdown;

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public IDocumentationDocument[] SubDocuments { get; set; } = null;

        public Markdig.Syntax.MarkdownDocument Markdown => Markdig.Markdown.Parse(System.IO.File.ReadAllText(Path), _MarkdownPipeline);

        private readonly MarkdownPipeline _MarkdownPipeline;

        public MarkdownDocument(MarkdownPipeline markdownPipeline)
        {
            _MarkdownPipeline = markdownPipeline;
        }

        public async Task<string> ToHtml(string baseUri)
        {
            return await Markdown.ToHtml(baseUri);
        }
        
        public Task<string> GetTitle()
        {
            HeadingBlock header = Markdown.FirstOrDefault(f => f is HeadingBlock) as HeadingBlock;

            if(header != null)
                return Task.FromResult(string.Join("", header.Inline));
            return Task.FromResult(Name);
        }


    }
}
