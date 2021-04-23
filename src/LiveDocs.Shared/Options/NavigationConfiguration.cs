namespace LiveDocs.Shared.Options
{
    public class NavigationConfiguration
    {
        public FooterNavigationElement[] Footer { get; set; }
        public HeaderNavigationElement[] Header { get; set; }

        public class FooterNavigationElement : NavigationElement
        {
        }

        public class HeaderNavigationElement : NavigationElement
        {
            public NavigationElement[] SubElements { get; set; }
        }

        public class NavigationElement
        {
            public bool NewTab { get; set; }
            public string Text { get; set; }
            public string Url { get; set; }
        }
    }
}