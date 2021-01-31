using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Search;
using LiveDocs.Shared.Services.Search.Configuration;
using LiveDocs.Shared.Tests.Services.Documents;
using Xunit;

namespace LiveDocs.Shared.Tests.Services.Search
{
    public class BasicSearchIndexTest
    {
        private readonly BasicSearchIndex basicSearchIndex;

        public BasicSearchIndexTest()
        {
            TestLiveDocsOptions options = new TestLiveDocsOptions
            {
                Search = new SearchConfiguration()
            };
            SearchPipeline searchPipeline = new SearchPipelineBuilder().Tokenize().Normalize().RemoveStopWords().Stem().Build();
            TestDocumentationIndex documentationIndex = new TestDocumentationIndex
            {
                DefaultProject = new TestDocumentationProject()
            };

            documentationIndex.DefaultProject.Documents.Add(new TestDocument());

            basicSearchIndex = new BasicSearchIndex(searchPipeline, documentationIndex, options)
            {
                Documents = new[]
                {
                    new BasicSearchIndex.Element
                    {
                        Path = "cat",
                        LexicalIndexes = new []{0,1,2}
                    }
                },
                Lexical = new[] { "cat", "are", "awesome" }
            };
        }

        [Theory]
        [InlineData("cat", new[] { "cat" })]
        [InlineData("cst", new[] { "cat" })]
        [InlineData("caa", new[] { "cat" })]
        [InlineData("awesome", new[] { "cat" })]
        [InlineData("awwsome", new[] { "cat" })]
        public async Task Search(string term, string[] matches)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            var result = await basicSearchIndex.Search(term, cts.Token);
            Assert.All(matches, (document) => result.Any(a => a.KeyPath == document));
        }
    }
}