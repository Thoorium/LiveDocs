using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Search;
using LiveDocs.Shared.Services.Search.Configuration;
using LiveDocs.Shared.Tests.Services.Documents;
using Xunit;
using Xunit.Abstractions;

namespace LiveDocs.Shared.Tests.Services.Search
{
    public class BigSearchIndexTest
    {
        private readonly BasicSearchIndex _BigSearchIndex;
        private readonly ITestOutputHelper _Output;
        private readonly Random _Random;

        public BigSearchIndexTest(ITestOutputHelper output)
        {
            _Output = output;
            TestLiveDocsOptions options = new TestLiveDocsOptions
            {
                Search = new SearchConfiguration()
            };
            SearchPipeline searchPipeline = new SearchPipelineBuilder().Tokenize().Normalize().RemoveStopWords().Stem().Build();
            TestDocumentationIndex documentationIndex = new TestDocumentationIndex
            {
                DefaultProject = new TestDocumentationProject()
            };

            _Random = new Random(127442);

            List<string> words = new List<string>();
            for (int i = 0; i < 15000; i++)
            {
                words.Add(WordMaker(_Random.Next(3, 10)));
            }

            // Seed a few english words
            words.Add("person");
            words.Add("year");
            words.Add("way");
            words.Add("day");
            words.Add("thing");
            words.Add("man");
            words.Add("world");
            words.Add("life");
            words.Add("hand");
            words.Add("cat");
            words.Add("code");

            for (int i = 0; i < 1000; i++)
            {
                List<string> content = new List<string>();
                for (int j = 0; j < _Random.Next(10, 500); j++)
                {
                    content.Add(words[_Random.Next(0, words.Count)]);
                }
                documentationIndex.DefaultProject.Documents.Add(new TestDocument(string.Join(' ', content)));
            }

            _BigSearchIndex = new BasicSearchIndex(searchPipeline, documentationIndex, options);
            _BigSearchIndex.BuildIndex().Wait();
        }

        [Theory]
        [InlineData("cat")]
        [InlineData("code cat")]
        [InlineData("cat person hand")]
        [InlineData("hand year world thing")]
        [InlineData("person life hand cat way")]
        [InlineData("man life day life thing person")]
        public async Task SearchPerformance(string term)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await _BigSearchIndex.Search(term, cts.Token);
            sw.Stop();
            _Output.WriteLine(sw.Elapsed.ToString());
            Assert.True(sw.ElapsedMilliseconds < 500);
        }

        // https://stackoverflow.com/questions/18110243/random-word-generator-2
        public string WordMaker(int requestedLength)
        {
            Random rnd = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string word = "";

            if (requestedLength == 1)
            {
                word = GetRandomLetter(rnd, vowels);
            } else
            {
                for (int i = 0; i < requestedLength; i += 2)
                {
                    word += GetRandomLetter(rnd, consonants) + GetRandomLetter(rnd, vowels);
                }

                word = word.Replace("q", "qu").Substring(0, requestedLength); // We may generate a string longer than requested length, but it doesn't matter if cut off the excess.
            }

            return word;
        }

        private static string GetRandomLetter(Random rnd, string[] letters)
        {
            return letters[rnd.Next(0, letters.Length - 1)];
        }
    }
}