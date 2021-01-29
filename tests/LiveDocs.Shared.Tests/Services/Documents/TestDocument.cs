using System;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;

namespace LiveDocs.Shared.Tests.Services.Documents
{
    public class TestDocument : IDocumentationDocument, ISearchableDocument
    {
        public DocumentationDocumentType DocumentType => DocumentationDocumentType.Unknown;
        public string FileName => "cat.txt";
        public string Key => "cat";
        public DateTime LastUpdate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name => "cat.txt";
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDocumentationDocument[] SubDocuments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SubDocumentsCount => throw new NotImplementedException();

        public Task<string> GetSearchableContent()
        {
            return Task.FromResult("Cat cat cat dog.");
        }

        public Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "")
        {
            throw new NotImplementedException();
        }
    }
}