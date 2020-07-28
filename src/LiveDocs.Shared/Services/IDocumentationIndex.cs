using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationIndex
    {
        IDocumentationProject DefaultProject { get; set; }
        IList<IDocumentationProject> Projects { get; set; }
        Task<bool> GetProjectFor(string[] path, out IDocumentationProject documentationProject, out string[] documentPath);
    }
}
