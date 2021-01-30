using LiveDocs.Shared.Services.Documents.Configuration;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteConfiguration
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public string ApplicationName { get; set; } = "LiveDocs";

        /// <summary>
        /// Index tree of the documentation documents
        /// </summary>
        public RemoteDocumentationIndex Documentation { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DocumentConfiguration Documents { get; set; } = new DocumentConfiguration();
    }
}