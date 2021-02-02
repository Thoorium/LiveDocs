using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Shared.Options;

namespace LiveDocs.Shared.Services.Search
{
    public class BasicSearchIndex : ISearchIndex
    {
        private IDocumentationIndex _DocumentationIndex;
        private ILiveDocsOptions _Options;
        private SearchPipeline _SearchPipeline;

        public BasicSearchIndex(SearchPipeline searchPipeline, IDocumentationIndex documentationIndex, ILiveDocsOptions options)
        {
            _SearchPipeline = searchPipeline;
            _DocumentationIndex = documentationIndex;
            _Options = options;
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
            Dictionary<Element, int> matchingDocuments = new Dictionary<Element, int>();
            foreach (var token in tokens)
            {
                var fuzzyMatch = FuzzyIndexesOf(Lexical, token);
                if (matchingDocuments.Count == 0)
                {
                    foreach (var match in fuzzyMatch)
                    {
                        foreach (var element in Documents)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return null;

                            if (element.LexicalIndexes.Contains(match.Index))
                            {
                                if (matchingDocuments.ContainsKey(element) && matchingDocuments[element] >= match.Distance)
                                    continue;

                                matchingDocuments[element] = match.Distance;
                            }
                        }
                    }
                } else
                {
                    Dictionary<Element, int> stillMatching = new Dictionary<Element, int>();
                    foreach (var match in fuzzyMatch)
                    {
                        foreach (var element in matchingDocuments.Keys)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return null;

                            if (element.LexicalIndexes.Contains(match.Index))
                            {
                                if (stillMatching.ContainsKey(element) && stillMatching[element] >= match.Distance)
                                    continue;

                                stillMatching[element] = match.Distance;
                            }
                        }
                    }
                    var ExistingElements = matchingDocuments.Keys.Intersect(stillMatching.Keys).ToArray();

                    // At this point if there's no match, no document contains any of the terms.
                    if (ExistingElements.Length == 0)
                    {
                        matchingDocuments.Clear();
                        break;
                    }

                    Dictionary<Element, int> temp = new Dictionary<Element, int>();

                    foreach (var existingElement in ExistingElements)
                    {
                        temp[existingElement] = matchingDocuments[existingElement] + stillMatching[existingElement];
                    }

                    matchingDocuments = temp;
                }
            }

            IList<BasicSearchResult> results = new List<BasicSearchResult>();

            foreach (var element in matchingDocuments)
            {
                await _DocumentationIndex.GetProjectFor(element.Key.Path.Split("/"), out var project, out var documentPath);

                var document = await project.GetDocumentFor(documentPath);
                if (document != null && !results.Any(a => a.KeyPath == element.Key.Path))
                {
                    results.Add(new BasicSearchResult
                    {
                        KeyPath = element.Key.Path,
                        Document = document,
                        HitCount = element.Value
                    });
                }
            }
            // TODO: Limit search result count?
            return results.OrderBy(o => o.HitCount).Select(s => (ISearchResult)s).ToList();
        }

        /// <summary>
        /// For setup post-deserialization
        /// </summary>
        /// <param name="searchPipeline"></param>
        /// <param name="documentationIndex"></param>
        public void Setup(SearchPipeline searchPipeline, IDocumentationIndex documentationIndex, ILiveDocsOptions options)
        {
            _SearchPipeline = searchPipeline;
            _DocumentationIndex = documentationIndex;
            _Options = options;
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
                int max_distance = (int)Math.Round(term.Length * _Options.Search.Tolerance, MidpointRounding.AwayFromZero);
                int distance = StringHelper.DamerauLevenshteinDistance(lexical[i], term, max_distance);
                if (distance != -1 && distance <= max_distance)
                {
                    matches.Add(new SearchMatch
                    {
                        Index = i,
                        Distance = distance
                    });
                }
            }
            return matches;
        }

        public class Element
        {
            public int[] LexicalIndexes { get; set; }
            public string Path { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || obj is not Element element)
                    return false;

                return Path == element.Path;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class SearchMatch
        {
            public int Distance { get; set; }

            public int Index { get; set; }
        }
    }
}