using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search.Filters
{
    public class NormalizeFilter : ISearchFilter
    {
        public Task<string[]> Apply(string[] input)
        {
            return Task.FromResult(input?.Select(s => s?.ToLower()).ToArray());
        }
    }
}