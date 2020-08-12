using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Remote
{
    public interface IRemoteDocumentationDocument : IDocumentationDocument
    {
        Task Cache();
    }
}