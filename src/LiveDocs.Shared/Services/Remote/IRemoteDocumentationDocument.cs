using System.Collections.Generic;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;

namespace LiveDocs.Shared.Services.Remote
{
    public interface IRemoteDocumentationDocument : IDocumentationDocument
    {
        Task<bool> TryCache();
        Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri);

        Task<List<DocumentTreeItem>> GetDocumentTree();
    }
}