using System.Collections.Generic;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;

namespace LiveDocs.Shared.Services.Remote
{
    public interface IRemoteDocumentationDocument : IDocumentationDocument
    {
        Task<List<DocumentTreeItem>> GetDocumentTree();

        Task<DocumentCacheResult> TryCache();

        Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri);
    }
}