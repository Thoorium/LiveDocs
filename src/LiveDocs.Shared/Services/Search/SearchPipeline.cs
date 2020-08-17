using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search
{
    public class SearchPipeline
    {
        private Dictionary<Type, ISearchFilter> filters = new Dictionary<Type, ISearchFilter>();

        public void AddOrUpdateFilter(ISearchFilter searchFilter)
        {
            if (searchFilter == null)
                return;

            var searchFilterType = searchFilter.GetType();
            filters[searchFilterType] = searchFilter;
        }

        public async Task<string[]> Analyse(string[] content)
        {
            foreach (var filter in filters)
            {
                content = await filter.Value.Apply(content);
            }

            return content;
        }
    }
}