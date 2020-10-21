namespace LiveDocs.Shared
{
    public class LinkRewriteResult
    {
        public LinkRewriteResult(string newUri)
        {
            NewUri = newUri;
        }

        /// <summary>
        /// The new uri to replace the one provided in the document.
        /// </summary>
        public string NewUri { get; }

        /// <summary>
        /// The relationship of the linked URL as space-separated link types.
        /// </summary>
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTML/Link_types"/>
        public string Rel { get; set; }

        /// <summary>
        /// Where to display the linked URL, as the name for a browsing context.
        /// </summary>
        public string Target { get; set; }
    }
}