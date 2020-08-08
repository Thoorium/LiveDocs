using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Documents;
using LiveDocs.Shared.Services.Search;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace LiveDocs.WebApp.Services
{
    public class LuceneSearchIndex : ISearchIndex
    {
        private readonly IDocumentationIndex DocumentationIndex;
        private IndexSearcher IndexSearcher;

        public LuceneSearchIndex(IDocumentationIndex documentationIndex)
        {
            DocumentationIndex = documentationIndex;
        }

        public async Task BuildIndex()
        {
            var AppLuceneVersion = LuceneVersion.LUCENE_48;
            var dir = new RAMDirectory();
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);

            await AddDocuments(writer, DocumentationIndex.DefaultProject.Documents);
            foreach (var project in DocumentationIndex.Projects)
            {
                List<string> paths = new List<string>
                {
                    project.KeyPath
                };
                await AddDocuments(writer, project.Documents, paths);
            }

            writer.Flush(triggerMerge: false, applyAllDeletes: false);
            IndexSearcher = new IndexSearcher(writer.GetReader(applyAllDeletes: true));
        }

        private async Task AddDocuments(IndexWriter writer, List<IDocumentationDocument> documentationDocuments, List<string> paths = null)
        {
            if (paths == null)
                paths = new List<string>();

            foreach (var document in documentationDocuments.Where(w => w is ISearchableDocument || w.DocumentType == DocumentationDocumentType.Folder))
            {
                if (document.DocumentType == DocumentationDocumentType.Folder && document.SubDocumentsCount > 0)
                {
                    List<string> tempFolderPaths = new List<string>(paths);
                    tempFolderPaths.Add(document.Key);
                    await AddDocuments(writer, document.SubDocuments.ToList(), tempFolderPaths);
                    continue;
                }

                ISearchableDocument searchableDocument = (ISearchableDocument)document;
                string content = await searchableDocument.GetContent();
                if (content.Length > 30000)
                    content = content.Substring(0, 30000);

                List<string> tempPaths = new List<string>(paths);
                tempPaths.Add(document.Key);
                Document doc = new Document
                {
                    new TextField("name", searchableDocument.Name, Field.Store.YES),
                    new TextField("content", content, Field.Store.YES),
                    new StringField("keypath", string.Join("/", tempPaths), Field.Store.YES)
                };
                writer.AddDocument(doc);
            }
        }

        public async Task<IList<ISearchResult>> Search(string term, CancellationToken cancellationToken)
        {
            List<ISearchResult> documents = new List<ISearchResult>();
            if (cancellationToken.IsCancellationRequested)
                return documents;

            string[] terms = term?.Trim().Split(" ");

            ScoreDoc[] nameHits = GetHits(terms, "name", cancellationToken);
            if (cancellationToken.IsCancellationRequested || nameHits == null)
                return documents;

            ScoreDoc[] contentHits = GetHits(terms, "content", cancellationToken);

            if (cancellationToken.IsCancellationRequested || contentHits == null)
                return documents;

            var hits = nameHits.Concat(contentHits).OrderByDescending(o => o.Score).Take(8);

            foreach (var hit in hits)
            {
                if (cancellationToken.IsCancellationRequested)
                    return documents;

                var foundDoc = IndexSearcher.Doc(hit.Doc);
                var keyPath = foundDoc.Get("keypath");

                await DocumentationIndex.GetProjectFor(keyPath.Split("/"), out var project, out var documentPath);

                var document = await project.GetDocumentFor(documentPath);
                if (document != null && !documents.Any(a => a.KeyPath == keyPath))
                {
                    documents.Add(new SearchResult
                    {
                        KeyPath = keyPath,
                        Document = document
                    });
                }
            }

            return documents;
        }

        private ScoreDoc[] GetHits(string[] terms, string field, CancellationToken cancellationToken)
        {
            List<SpanQuery> spanQueries = new List<SpanQuery>();
            foreach (var cleanTerm in terms)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                spanQueries.Add(new SpanMultiTermQueryWrapper<FuzzyQuery>(new FuzzyQuery(new Term(field, cleanTerm), maxEdits: 1)));
            }

            SpanNearQuery query = new SpanNearQuery(spanQueries.ToArray(), 0, true);

            if (spanQueries.Count == 0)
                return null;

            var hits = IndexSearcher.Search(query, 8).ScoreDocs;
            return hits;
        }
    }
}
