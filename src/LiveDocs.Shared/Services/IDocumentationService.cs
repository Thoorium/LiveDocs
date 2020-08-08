using System.Threading.Tasks;
using LiveDocs.Shared.Services.Search;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationService
    {
        IDocumentationIndex DocumentationIndex { get; set; }
        ISearchIndex SearchIndex { get; set; }

        Task<IDocumentationIndex> IndexFiles();

        Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex);

        Task RefreshSearchIndex(IDocumentationIndex documentationIndex);

    }
}
