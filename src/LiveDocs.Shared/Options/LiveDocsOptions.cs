using LiveDocs.Shared.Services.Documents.Configuration;
using LiveDocs.Shared.Services.Search.Configuration;

namespace LiveDocs.Shared.Options
{
    public class LiveDocsOptions : ILiveDocsOptions
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public string ApplicationName { get; set; } = "LiveDocs";

        /// <summary>
        /// Name of the documents, in order of priority, to use when no path is provided.
        /// </summary>
        public string[] DefaultDocuments { get; set; }

        /// <summary>
        /// Path to the root folder containing the documentation.
        /// </summary>
        public string DocumentationFolder { get; set; }

        /// <summary>
        /// Configuration applied to documents.
        /// </summary>
        public DocumentConfiguration Documents { get; set; }

        /// <summary>
        /// Name of the document to use as a landing page. If empty, no landing page is shown.
        /// </summary>
        public string LandingPageDocument { get; set; }

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