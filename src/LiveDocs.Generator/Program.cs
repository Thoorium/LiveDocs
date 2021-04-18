using LiveDocs.Generator.Services;
using LiveDocs.Shared.Options;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiveDocs.Generator
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables(prefix: "LiveDocs_");
                    config.AddCommandLine(args);
                })
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddJsonFile("livedocs.json");
                    configHost.AddJsonFile("livedocs.Development.json", optional: true);
                    configHost.AddJsonFile("livedocs.Staging.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<LiveDocsOptions>(hostContext.Configuration.GetSection("livedocs"));

                    SearchPipeline searchPipeline = new SearchPipelineBuilder().Tokenize().Normalize().RemoveStopWords().Stem().Build();
                    services.AddSingleton(searchPipeline);

                    services.AddSingleton<IDocumentationService, DocumentationService>();

                    services.AddHostedService<Worker>();
                });

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}