using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search
{
    public interface ISearchFilter
    {
        Task<string[]> Apply(string[] input);
    }
}