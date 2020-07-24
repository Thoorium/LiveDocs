using System.Collections.Generic;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationIndex
    {
        List<IDocumentationDocument> DefaultDocuments { get; set; }
        List<IDocumentationDocument> Documents { get; set; }
        IDocumentationDocument LandingPage { get; set; }
    }
}
