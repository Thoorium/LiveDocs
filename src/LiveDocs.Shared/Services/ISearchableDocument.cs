using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface ISearchableDocument
    {
        public string Name { get; }

        public Task<string> GetContent();
    }
}