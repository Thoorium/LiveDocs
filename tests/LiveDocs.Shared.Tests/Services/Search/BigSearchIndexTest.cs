using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            for (int i = 0; i < 1000; i++)
            {
                List<string> words = new List<string>();
                for (int j = 0; j < _Random.Next(10, 500); j++)
                {
                    words.Add(WordMaker(_Random.Next(3, 10)));
                }

                documentationIndex.DefaultProject.Documents.Add(new TestDocument(string.Join(' ', words)));
            }

            //documentationIndex.DefaultProject.Documents.Add(new TestDocument("Cat cat cat dog."));

            _BigSearchIndex = new BasicSearchIndex(searchPipeline, documentationIndex, options);
            _BigSearchIndex.BuildIndex().Wait();
        }

        [Fact]
        public async Task Search()
        {
            List<string> words = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                words.Add(_BigSearchIndex.Lexical[_Random.Next(0, _BigSearchIndex.Lexical.Length)]);
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await _BigSearchIndex.Search(words[0], cts.Token);
            sw.Stop();
            _Output.WriteLine(sw.Elapsed.ToString());
            sw.Reset();
            sw.Start();
            await _BigSearchIndex.Search(string.Join(' ', words.Take(2)), cts.Token);
            _Output.WriteLine(sw.Elapsed.ToString());
            sw.Start();
            await _BigSearchIndex.Search(string.Join(' ', words.Take(3)), cts.Token);
            _Output.WriteLine(sw.Elapsed.ToString());
            sw.Start();
            await _BigSearchIndex.Search(string.Join(' ', words.Take(4)), cts.Token);
            _Output.WriteLine(sw.Elapsed.ToString());
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