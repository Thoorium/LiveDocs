using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationService
    {
        IDocumentationIndex DocumentationIndex { get; set; }

        Task<IDocumentationIndex> IndexFiles();

        Task RefreshDocumentationIndex(IDocumentationIndex documentationIndex);

        Task<ISearchIndex> RefreshSearchIndex(IDocumentationIndex documentationIndex);

    }
}
