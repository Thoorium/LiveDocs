using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LiveDocs.Shared.Services;
using LiveDocs.Client.Services;
using Markdig;

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
            builder.Services.AddSingleton<IDocumentationService, RemoteDocumentationService>();

            await builder.Build().RunAsync();
        }
    }
}
