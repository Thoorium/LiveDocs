using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemotePdfDocument : PdfDocument, IRemoteDocumentationDocument
    {
        public Task Cache()
        {
            return Task.CompletedTask;
        }
    }
}