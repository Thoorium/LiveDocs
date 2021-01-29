using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;

namespace LiveDocs.Shared.Tests.Services
{
    public class TestDocumentationProject : IDocumentationProject
    {
        public List<IDocumentationDocument> DefaultDocuments { get; set; } = new List<IDocumentationDocument>();

        public int DocumentCount => throw new NotImplementedException();

        public List<IDocumentationDocument> Documents { get; set; } = new List<IDocumentationDocument>();

        public string Key => throw new NotImplementedException();

        public string KeyPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IDocumentationDocument LandingPage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => throw new NotImplementedException();

        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<IDocumentationProject> SubProjects { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<IDocumentationDocument> GetDocumentationDefaultDocument(string documentType = "")
        {
            throw new NotImplementedException();
        }

        public Task<IDocumentationDocument[]> GetDocumentationDefaultDocuments(string documentType = "")
        {
            throw new NotImplementedException();
        }

        public Task<IDocumentationDocument> GetDocumentationLandingPageDocument()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFirstAvailableDocumentPath()
        {
            throw new NotImplementedException();
        }
    }
}