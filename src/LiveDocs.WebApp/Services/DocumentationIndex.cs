using LiveDocs.Shared.Services;
using System.Collections.Generic;

namespace LiveDocs.WebApp.Services
{
    public class DocumentationIndex : IDocumentationIndex
    {
        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();
    }
}
