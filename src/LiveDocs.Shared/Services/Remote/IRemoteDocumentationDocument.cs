using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Remote
{
    public interface IRemoteDocumentationDocument : IDocumentationDocument
    {
        Task<bool> TryCache();
        Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri);
    }
}