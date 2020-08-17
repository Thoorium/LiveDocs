using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Search.Filters.Stemming;

namespace LiveDocs.Shared.Services.Search.Filters
{
    public class StemmingFilter : ISearchFilter
    {
        public Task<string[]> Apply(string[] input)
        {
            EnglishPorter2Stemmer stemmer = new EnglishPorter2Stemmer();
            return Task.FromResult(input?.Select(s => stemmer.Stem(s)).Where(w => !string.IsNullOrWhiteSpace(w)).ToArray());
        }
    }
}