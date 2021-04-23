using System.Threading;
using System.Threading.Tasks;
using LiveDocs.Shared.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiveDocs.Generator
{
    public class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _ApplicationLifetime;
        private readonly IDocumentationService _DocumentationService;
        private readonly ILogger<Worker> _Logger;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime, IDocumentationService documentationService)
        {
            _Logger = logger;
            _ApplicationLifetime = applicationLifetime;
            _DocumentationService = documentationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var documentationIndex = await _DocumentationService.IndexFiles();

            await _DocumentationService.RefreshDocumentationIndex(documentationIndex);
            await _DocumentationService.RefreshSearchIndex(documentationIndex);
            _ApplicationLifetime.StopApplication();
        }
    }
}