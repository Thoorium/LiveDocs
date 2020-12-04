using System.IO;
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
                "drawio" => DocumentationDocumentType.Drawio,
                "drawio.svg" => DocumentationDocumentType.DrawioSvg,
                "ldproject" => DocumentationDocumentType.Project,
                "ldversion" => DocumentationDocumentType.Version,
                _ => DocumentationDocumentType.Unknown
            };
        }

        public static DocumentationDocumentType GetDocumentationDocumentTypeFromName(string name)
        {
            if (name?.EndsWith(".drawio.svg", System.StringComparison.InvariantCultureIgnoreCase) == true)
                return DocumentationDocumentType.DrawioSvg;

            return GetDocumentationDocumentTypeFromExtension(Path.GetExtension(name));
        }

        public static DocumentationDocumentType GetDocumentationDocumentTypeFromString(string documentationType)
        {
            return documentationType.ToLower() switch
            {
                "markdown" => DocumentationDocumentType.Markdown,
                "html" => DocumentationDocumentType.Html,
                "pdf" => DocumentationDocumentType.Pdf,
                "word" => DocumentationDocumentType.Word,
                "drawio" => DocumentationDocumentType.Drawio,
                "drawiosvg" => DocumentationDocumentType.DrawioSvg,
                "drawio.svg" => DocumentationDocumentType.DrawioSvg,
                _ => DocumentationDocumentType.Unknown
            };
        }

        public static string GetDocumentNameWithoutExtension(string fullname)
        {
            if (fullname?.EndsWith(".drawio.svg", System.StringComparison.InvariantCultureIgnoreCase) == true)
                return Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fullname));

            return Path.GetFileNameWithoutExtension(fullname);
        }
    }
}