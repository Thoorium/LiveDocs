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
        public string FileName => System.IO.Path.GetFileName(Path);
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public Markdig.Syntax.MarkdownDocument Markdown => Markdig.Markdown.Parse(System.IO.File.ReadAllText(Path), _MarkdownPipeline);

        private readonly MarkdownPipeline _MarkdownPipeline;

        public MarkdownDocument(MarkdownPipeline markdownPipeline)
        {
            _MarkdownPipeline = markdownPipeline;
        }

        public async Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            return await Markdown.ToHtml(documentationProject, baseUri);
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
