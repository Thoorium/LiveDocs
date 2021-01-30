using System;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Client.Services;
using LiveDocs.Shared.Services;
using LiveDocs.Shared.Services.Remote;
using LiveDocs.Shared.Services.Search;
using Markdig;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseBootstrap().Build();
            builder.Services.AddSingleton(markdownPipeline);

            SearchPipeline searchPipeline = new SearchPipelineBuilder().Tokenize().Normalize().RemoveStopWords().Stem().Build();
            builder.Services.AddSingleton(searchPipeline);

            builder.Services.AddSingleton<IDocumentationService, RemoteDocumentationService>();

            builder.Services.AddSingleton<RemoteConfiguration>();

            await builder.Build().RunAsync();
        }
    }
}