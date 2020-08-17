using System.Collections.Generic;
using LiveDocs.Shared.Services.Search.Filters;

namespace LiveDocs.Shared.Services.Search
{
    public class SearchPipelineBuilder
    {
        private List<ISearchFilter> filters = new List<ISearchFilter>();

        public SearchPipeline Build()
        {
            SearchPipeline searchPipeline = new SearchPipeline();
            foreach (var filter in filters)
            {
                searchPipeline.AddOrUpdateFilter(filter);
            }
            return searchPipeline;
        }

        /// <summary>
        /// Set everything to lowercase.
        /// </summary>
        /// <returns></returns>
        public SearchPipelineBuilder Normalize()
        {
            filters.Add(new NormalizeFilter());
            return this;
        }

        /// <summary>
        /// Remove common words.
        /// </summary>
        /// <returns></returns>
        public SearchPipelineBuilder RemoveStopWords()
        {
            filters.Add(new StopWordRemoverFilter());
            return this;
        }

        /// <summary>
        /// Change words to their simplest expression. Ex: fishing, fished and fisher becomes fish.
        /// </summary>
        /// <returns></returns>
        public SearchPipelineBuilder Stem()
        {
            filters.Add(new StemmingFilter());
            return this;
        }
        /// <summary>
        /// Split the string into word tokens.
        /// </summary>
        /// <returns></returns>
        public SearchPipelineBuilder Tokenize()
        {
            filters.Add(new TokenizerFilter());
            return this;
        }
    }
}