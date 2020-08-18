using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;

namespace LiveDocs.Server.Services
{
    public class DocumentationIndex : IDocumentationIndex
    {
        public IDocumentationProject DefaultProject { get; set; }
        public IList<IDocumentationProject> Projects { get; set; } = new List<IDocumentationProject>();

        public Task<bool> GetProjectFor(string[] path, out IDocumentationProject documentationProject, out string[] documentPath)
        {
            // Remove empty entries
            path = path.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            documentPath = path;
            if (path.Length == 0)
            {
                documentationProject = DefaultProject;
                return Task.FromResult(false);
            }

            var projects = Projects.Where(w => w.Key == path[0]);

            if (!projects.Any())
            {
                documentationProject = DefaultProject;
                return Task.FromResult(false);
            }

            string finalKey = path[0];

            IDocumentationProject project = projects.FirstOrDefault(w => w.Key == path[0]);
            documentPath[0] = "";
            for (int i = 1; i < path.Length; i++)
            {
                var tempProject = project.SubProjects.FirstOrDefault(w => w.Key == path[i]);

                if (tempProject == null)
                {
                    documentPath = documentPath.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
                    documentationProject = project ?? DefaultProject;
                    return Task.FromResult(false);
                }
                documentPath[i] = "";
                project = tempProject;
            }

            documentPath = documentPath.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            documentationProject = project;
            return Task.FromResult(true);
        }
    }
}