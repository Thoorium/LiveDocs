using System;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;

namespace LiveDocs.Shared.Tests.Services.Documents
{
    public class TestDocument : IDocumentationDocument, ISearchableDocument
    {
        private readonly string _Content;
        private readonly string _Key;

        public TestDocument(string content)
        {
            _Content = content;
            _Key = Guid.NewGuid().ToString();
        }

        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Unknown;
        public string FileName => $"{_Key}.txt";
        public string Key => _Key;
        public DateTime LastUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name => $"{_Key}.txt";
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDocumentationDocument[] SubDocuments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SubDocumentsCount => throw new NotImplementedException();

        public Task<string> GetSearchableContent()
        {
            return Task.FromResult(_Content);
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            throw new NotImplementedException();
        }
    }
}