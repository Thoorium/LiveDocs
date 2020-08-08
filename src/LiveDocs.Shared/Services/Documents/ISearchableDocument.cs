using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Documents
{
    public interface ISearchableDocument
    {
        string Name { get; }
        public Task<string> GetContent();
    }
}
