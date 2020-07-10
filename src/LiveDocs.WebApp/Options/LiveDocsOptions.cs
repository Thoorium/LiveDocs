namespace LiveDocs.WebApp.Options
{
    public class LiveDocsOptions
    {
        public string ApplicationName { get; set; } = "LiveDocs";

        public string[] DefaultDocuments { get; set; }

        public string DocumentationFolder { get; set; }

        public string LandingPageDocument { get; set; }
    }
}
