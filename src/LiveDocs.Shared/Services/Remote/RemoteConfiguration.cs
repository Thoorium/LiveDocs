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
        /// Show the "Download Original" download link above documents.
        /// </summary>
        public bool ShowDownloadOriginal { get; set; }
    }
}
