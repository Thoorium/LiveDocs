using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search
{
    public interface ISearchIndex
    {
        public Task<IList<ISearchResult>> Search(string term, CancellationToken cancellationToken);
        public Task BuildIndex();
    }
}
