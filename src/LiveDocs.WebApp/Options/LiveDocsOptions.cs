namespace LiveDocs.WebApp.Options
{
    public class LiveDocsOptions
    {
        public string ApplicationName { get; set; } = "LiveDocs";

        public string DocumentationFolder { get; set; }

        public string[] DefaultDocuments { get; set; }
    }
}
