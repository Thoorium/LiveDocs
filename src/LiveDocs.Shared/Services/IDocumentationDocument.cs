using System;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationDocument
    {
        string Key { get; }
        string Name { get; }
        string Path { get; set; }
        DateTime LastUpdate { get; set; }
        DocumentationDocumentType DocumentType { get; }

        IDocumentationDocument[] SubDocuments { get; set; }

        Task<string> ToHtml(string baseUri = "");

        Task<string> GetTitle() => Task.FromResult(Name);
    }
}
