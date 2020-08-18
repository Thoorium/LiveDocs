using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationDocument
    {
        [JsonIgnore]
        DocumentationDocumentType DocumentType { get; }

        [JsonIgnore]
        string FileName { get; }

        [JsonIgnore]
        string Key { get; }

        DateTime LastUpdate { get; set; }

        [JsonIgnore]
        string Name { get; }

        string Path { get; set; }
        IDocumentationDocument[] SubDocuments { get; set; }

        [JsonIgnore]
        int SubDocumentsCount { get; }

        Task<string> GetTitle() => Task.FromResult(Name);

        Task<string> ToHtml(IDocumentationProject documentationProject, string baseUri = "");
    }
}