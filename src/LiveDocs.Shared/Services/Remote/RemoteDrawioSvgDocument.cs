using System.Collections.Generic;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteDrawioSvgDocument : DrawioSvgDocument, IRemoteDocumentationDocument
    {
        public Task<List<DocumentTreeItem>> GetDocumentTree()
        {
            throw new System.NotImplementedException();
        }

        public Task<DocumentCacheResult> TryCache()
        {
            return Task.FromResult(DocumentCacheResult.Success);
        }

        public async Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            var htmlString = await ToHtml(documentationProject, baseUri);
            return (true, htmlString);
        }
    }
}