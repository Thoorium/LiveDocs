using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Controllers
{
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _HostEnvironment;
        private readonly LiveDocsOptions _Options;
        private readonly IDocumentationService _DocumentationService;
        private readonly ILogger<FileController> _Logger;

        public FileController(ILogger<FileController> logger, IWebHostEnvironment hostEnvironment, IOptions<LiveDocsOptions> options, IDocumentationService documentationService)
        {
            _Logger = logger;
            _HostEnvironment = hostEnvironment;
            _Options = options.Value;
            _DocumentationService = documentationService;
        }

        [HttpGet("{path1}.{ext}")]
        [HttpGet("{path1}/{path2}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}/{path4}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}/{path4}/{path5}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}/{path4}/{path5}/{path6}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}/{path4}/{path5}/{path6}/{path7}.{ext}")]
        [HttpGet("{path1}/{path2}/{path3}/{path4}/{path5}/{path6}/{path7}/{path8}.{ext}")]
        public async Task<IActionResult> DownloadFile(string path1, string path2, string path3, string path4, string path5, string path6, string path7, string path8, string ext)
        {
            var paths = new[] { path1, path2, path3, path4, path5, path6, path7, path8 }.Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            string fullFilename = string.Join("/", paths) + "." + ext;
            var isDefaultProject = _DocumentationService.DocumentationIndex.GetProjectFor(paths, out IDocumentationProject currentProject, out string[] documentPath);
            IDocumentationDocument document = await currentProject.GetDocumentFor(documentPath, ext);

            string path;
            if (document != null)
                path = document.Path;
            else path = Path.Combine(_Options.GetDocumentationFolderAsAbsolute(_HostEnvironment.ContentRootPath).FullName, fullFilename);

            var found = System.IO.File.Exists(path);

            if (!found && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                found = TryFindFileCaseInsensitive();

            if (!found)
                return NotFound($"Not found: {fullFilename}");

            var fileStream = new FileStream(path, FileMode.Open);

            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            bool forceDownload = false;
            if (!provider.TryGetContentType(fullFilename, out contentType))
            {
                contentType = "application/octet-stream";
                forceDownload = true;
            }

            var result = new FileStreamResult(fileStream, contentType);
            // If the name is specified, the browser will automatically try to download the file. Sometimes.
            if (forceDownload)
                result.FileDownloadName = fullFilename;
            return result;

            bool TryFindFileCaseInsensitive()
            {
                _Logger.LogInformation($"Searching for file {string.Join("/", paths)}.{ext} with insensitive case.");

                var dirInfo = new DirectoryInfo(_Options.GetDocumentationFolderAsAbsolute(_HostEnvironment.ContentRootPath).FullName);
                var dirEnumOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };

                // Manually scan the path to the file due to case sensitivity.
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    var subDirInfo = dirInfo.GetDirectories(paths[i], dirEnumOptions);
                    _Logger.LogDebug($"Found {subDirInfo.Length} matching directory for path {dirInfo.FullName}/{paths[i]}.");

                    if (subDirInfo.Length == 0)
                        return false;

                    dirInfo = subDirInfo.First();
                }
                var fileInfos = dirInfo.GetFiles($"{paths[^1]}.{ext}", dirEnumOptions);
                _Logger.LogDebug($"Found {fileInfos.Length} matching file for path {dirInfo.FullName}/{paths[^1]}.{ext}.");

                if (fileInfos.Length == 0)
                    return false;

                path = fileInfos.First().FullName;
                return true;
            }
        }
    }
}
