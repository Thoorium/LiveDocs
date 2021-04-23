using LiveDocs.Shared.Services.Documents.Configuration;
using LiveDocs.Shared.Services.Remote;
using LiveDocs.Shared.Services.Search.Configuration;

namespace LiveDocs.Shared.Options
{
    public class RemoteLiveDocsOptions : ILiveDocsOptions
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Index tree of the documentation documents
        /// </summary>
        public RemoteDocumentationIndex Documentation { get; set; }

        /// <summary>
        ///Configuration applied to the documents.
        /// </summary>
        public DocumentConfiguration Documents { get; set; }

        /// <summary>
        /// Configuration applied to menu elements and other navigation components.
        /// </summary>
        public NavigationConfiguration Navigation { get; set; }

        /// <summary>
        /// Configuration applied to the search.
        /// </summary>
        public SearchConfiguration Search { get; set; }
    }
}