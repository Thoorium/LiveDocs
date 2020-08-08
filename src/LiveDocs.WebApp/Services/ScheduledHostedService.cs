using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services
{
    public class ScheduledHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<ScheduledHostedService> _Logger;
        private readonly IServiceProvider _Services;
        private readonly LiveDocsOptions _Options;

        //private Timer fastTimer;
        private Timer slowTimer;

        public ScheduledHostedService(IServiceProvider services, ILogger<ScheduledHostedService> logger, IOptions<LiveDocsOptions> options)
        {
            _Logger = logger;
            _Services = services;
            _Options = options.Value;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("Scheduled Hosted Service is running.");

            //fastTimer = new Timer(async (object state) => await FastDoWork(stoppingToken, state), null, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(500));
            slowTimer = new Timer(async (object state) => await SlowDoWork(stoppingToken, state), null, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        //private async Task FastDoWork(CancellationToken stoppingToken, object state)
        //{
        //    using (var scope = _Services.CreateScope())
        //    {
        //    }
        //}

        private async Task SlowDoWork(CancellationToken stoppingToken, object state)
        {
            using (var scope = _Services.CreateScope())
            {
                var index = scope.ServiceProvider.GetRequiredService<IDocumentationService>();
                var documentationIndex = await index.IndexFiles();

                await index.RefreshDocumentationIndex(documentationIndex);
                await index.RefreshSearchIndex(documentationIndex);
            }
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("Scheduled Hosted Service is stopping.");

            //fastTimer?.Change(Timeout.Infinite, 0);
            slowTimer?.Change(Timeout.Infinite, 0);

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            //fastTimer?.Dispose();
            slowTimer?.Dispose();
        }
    }
}
