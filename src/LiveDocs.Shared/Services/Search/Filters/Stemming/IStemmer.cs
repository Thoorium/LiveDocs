// SOURCE: https://github.com/nemec/porter2-stemmer
// AUTHOR: nemec
// DATE: 2020-08-15

namespace LiveDocs.Shared.Services.Search.Filters.Stemming
{
    public interface IStemmer
    {
        /// <summary>
        /// Stem a word.
        /// </summary>
        /// <param name="word">Word to stem.</param>
        /// <returns>
        /// The stemmed word, with a reference to the original unstemmed word.
        /// </returns>
        string Stem(string word);
    }
}