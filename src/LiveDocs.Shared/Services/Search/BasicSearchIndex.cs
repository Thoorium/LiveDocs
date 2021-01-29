using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search
{
    public class BasicSearchIndex : ISearchIndex
    {
        private IDocumentationIndex _DocumentationIndex;

        private SearchPipeline _SearchPipeline;

        public BasicSearchIndex(SearchPipeline searchPipeline, IDocumentationIndex documentationIndex)
        {
            _SearchPipeline = searchPipeline;
            _DocumentationIndex = documentationIndex;
        }

        /// <summary>
        /// For deserialization. Call <c>Setup</c> after.
        /// </summary>
        public BasicSearchIndex()
        {
        }

        public Element[] Documents { get; set; }

        public string[] Lexical { get; set; }

        public async Task BuildIndex()
        {
            List<Element> elements = new List<Element>();
            List<string> lexical = new List<string>();
            await AddDocuments(lexical, elements, _DocumentationIndex.DefaultProject.Documents);
            foreach (var project in _DocumentationIndex.Projects)
            {
                List<string> paths = new List<string>
                {
                    project.KeyPath
                };
                await AddDocuments(lexical, elements, project.Documents, paths);
            }

            Lexical = lexical.ToArray();
            Documents = elements.ToArray();
        }

        public async Task<IList<ISearchResult>> Search(string term, CancellationToken cancellationToken)
        {
            var tokens = await _SearchPipeline.Analyse(new string[] { term });
            List<List<SearchMatch>> tokenMatches = new List<List<SearchMatch>>();
            foreach (var token in tokens)
            {
                var fuzzyMatch = FuzzyIndexesOf(Lexical, token);
                tokenMatches.Add(fuzzyMatch);
            }
            var matchGroup = tokenMatches.CartesianProduct();

            // For debug purposes.
            // TODO: Remove.
            //Console.WriteLine(string.Join(',', tokens));
            //int[] lexicalIds1 = tokens.Select(s => Array.IndexOf(Lexical, s)).ToArray();
            //Console.WriteLine("Direct: " + string.Join(',', lexicalIds1.Select(s => s.ToString())));
            //foreach (var match in matchGroup)
            //{
            //    Console.WriteLine("Fuzzy : " + string.Join(',', match.Select(s => s.Index)));
            //}

            IList<BasicSearchResult> results = new List<BasicSearchResult>();

            foreach (var match in matchGroup.OrderBy(o => o.Sum(su => su.Distance)))
            {
                foreach (var element in Documents)
                {
                    var lexicalIds = match.Select(s => s.Index);
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    var matches = lexicalIds.Intersect(element.LexicalIndexes).ToArray();

                    // Inclusive search
                    if (lexicalIds.Any(a => !matches.Contains(a)))
                        continue;

                    await _DocumentationIndex.GetProjectFor(element.Path.Split("/"), out var project, out var documentPath);

                    var document = await project.GetDocumentFor(documentPath);
                    if (document != null && !results.Any(a => a.KeyPath == element.Path))
                    {
                        results.Add(new BasicSearchResult
                        {
                            KeyPath = element.Path,
                            Document = document,
                            HitCount = matches.Length
                        });
                    }
                }
            }
            // TODO: Limit search result count?
            return results.OrderByDescending(o => o.HitCount).Select(s => (ISearchResult)s).ToList();
        }

        /// <summary>
        /// For setup post-deserialization
        /// </summary>
        /// <param name="searchPipeline"></param>
        /// <param name="documentationIndex"></param>
        public void Setup(SearchPipeline searchPipeline, IDocumentationIndex documentationIndex)
        {
            _SearchPipeline = searchPipeline;
            _DocumentationIndex = documentationIndex;
        }

        private async Task AddDocuments(List<string> lexical, List<Element> elements, List<IDocumentationDocument> documentationDocuments, List<string> paths = null)
        {
            if (paths == null)
                paths = new List<string>();

            foreach (var document in documentationDocuments.Where(w => w is ISearchableDocument || w.DocumentType == DocumentationDocumentType.Folder))
            {
                if (document.DocumentType == DocumentationDocumentType.Folder && document.SubDocumentsCount > 0)
                {
                    List<string> tempFolderPaths = new List<string>(paths)
                    {
                        document.Key
                    };
                    await AddDocuments(lexical, elements, document.SubDocuments.ToList(), tempFolderPaths);
                    continue;
                }

                ISearchableDocument searchableDocument = (ISearchableDocument)document;
                string content = await searchableDocument.GetSearchableContent();
                var tokens = await _SearchPipeline.Analyse(new string[] { document.Name, content });
                List<string> tempPaths = new List<string>(paths)
                {
                    document.Key
                };
                var documentFullPath = string.Join("/", tempPaths);

                foreach (var token in tokens)
                {
                    if (!lexical.Contains(token))
                        lexical.Add(token);
                }

                elements.Add(new Element
                {
                    Path = documentFullPath,
                    LexicalIndexes = tokens.Select(s => lexical.IndexOf(s)).Distinct().ToArray()
                }); ;
            }
        }

        private List<SearchMatch> FuzzyIndexesOf(string[] lexical, string term)
        {
            List<SearchMatch> matches = new List<SearchMatch>();
            for (int i = 0; i < lexical.Length; i++)
            {
                int distance = StringHelper.DamerauLevenshteinDistance(lexical[i], term);
                // TODO: Configurable distance.
                int max_distance = (int)Math.Round(term.Length / 4d, MidpointRounding.AwayFromZero);
                if (distance <= max_distance)
                    matches.Add(new SearchMatch
                    {
                        Index = i,
                        Distance = distance
                    });
            }
            return matches;
        }

        public class Element
        {
            public int[] LexicalIndexes { get; set; }
            public string Path { get; set; }
        }

        private class SearchMatch
        {
            public int Distance { get; set; }

            public int Index { get; set; }
        }
    }
}