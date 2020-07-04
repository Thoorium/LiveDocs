using System;
using System.Text.RegularExpressions;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationDocument
    {
        string Key => Markdig.Helpers.LinkHelper.Urilize(Name, allowOnlyAscii: true);
        string Name { get; }
        string Path { get; set; }
        DateTime LastUpdate { get; set; }
        DocumentationDocumentType DocumentType { get; set; }

        IDocumentationDocument[] SubDocuments { get; set; }
    }
}
