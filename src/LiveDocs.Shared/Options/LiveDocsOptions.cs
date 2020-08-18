namespace LiveDocs.Shared.Options
{
    public class LiveDocsOptions
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
        /// Name of the document to use as a landing page. If empty, no landing page is shown.
        /// </summary>
        public string LandingPageDocument { get; set; }
    }
}