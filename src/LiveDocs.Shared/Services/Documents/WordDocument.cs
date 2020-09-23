using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Thoorium.WordLib;

namespace LiveDocs.Shared.Services.Documents
{
    public class WordDocument : IDocumentationDocument, ISearchableDocument
    {
        public virtual Thoorium.WordLib.WordDocument Document => Thoorium.WordLib.WordDocument.Load(Path);
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Markdown;
        public string FileName => System.IO.Path.GetFileName(Path);
        public string Key => UrlHelper.Urilize(Name);
        public DateTime LastUpdate { get; set; }
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
        public string Path { get; set; }
        public IDocumentationDocument[] SubDocuments { get; set; } = null;
        public int SubDocumentsCount => (SubDocuments?.Count(c => c.DocumentType != DocumentationDocumentType.Project && c.DocumentType != DocumentationDocumentType.Project) ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;

        public async Task<string> GetContent()
        {
            return await GetContent(Document.Elements);
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            WordPipeline wordPipeline = new WordPipelineBuilder().UseBootstrap().Build();
            return WordConverter.ConvertToHtml(Document, wordPipeline);
        }

        private async Task<string> GetContent(IEnumerable<IWordElement> wordElements)
        {
            string content = "";
            foreach (var wordElement in wordElements)
            {
                if (wordElement.InnerElements?.Count > 0)
                    content += await GetContent(wordElement.InnerElements);
                content += $" {wordElement.Value}";
            }

            return content;
        }
    }
}