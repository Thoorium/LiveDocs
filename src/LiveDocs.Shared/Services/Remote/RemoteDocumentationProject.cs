using System.Collections.Generic;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteDocumentationProject
    {
        public string KeyPath { get; set; }
        public string Path { get; set; }
        public List<RemoteDocumentationDocument> DefaultDocuments { get; set; }
        public List<RemoteDocumentationDocument> Documents { get; set; }
        public List<RemoteDocumentationDocument> SubProjects { get; set; }
        public RemoteDocumentationDocument LandingPage { get; set; }
    }
}