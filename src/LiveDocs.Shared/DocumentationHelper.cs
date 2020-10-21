using LiveDocs.Shared.Services;

namespace LiveDocs.Shared
{
    public static class DocumentationHelper
    {
        public static DocumentationDocumentType GetDocumentationDocumentTypeFromExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension) || extension == ".")
                return DocumentationDocumentType.Unknown;

            extension = extension[1..].ToLower();

            return extension switch
            {
                "htm" => DocumentationDocumentType.Html,
                "html" => DocumentationDocumentType.Html,
                "md" => DocumentationDocumentType.Markdown,
                "pdf" => DocumentationDocumentType.Pdf,
                //"doc" => DocumentationDocumentType.Word,
                "docx" => DocumentationDocumentType.Word,
                "ldproject" => DocumentationDocumentType.Project,
                "ldversion" => DocumentationDocumentType.Version,
                _ => DocumentationDocumentType.Unknown
            };
        }

        public static DocumentationDocumentType GetDocumentationDocumentTypeFromString(string documentationType)
        {
            return documentationType.ToLower() switch
            {
                "markdown" => DocumentationDocumentType.Markdown,
                "html" => DocumentationDocumentType.Html,
                "pdf" => DocumentationDocumentType.Pdf,
                "word" => DocumentationDocumentType.Word,
                _ => DocumentationDocumentType.Unknown
            };
        }
    }
}
