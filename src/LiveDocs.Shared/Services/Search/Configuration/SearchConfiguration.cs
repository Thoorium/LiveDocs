namespace LiveDocs.Shared.Services.Search.Configuration
{
    public class SearchConfiguration : ISearchConfiguration
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Error tolerance per character. Cumulative, rounded up.
        /// </summary>
        /// <example>0.25 gives 1 mistake for any word from 1 to 4 characters. 5 to 8, 2 mistakes. And so on.</example>
        public double Tolerance { get; set; } = 0.25d;
    }
}