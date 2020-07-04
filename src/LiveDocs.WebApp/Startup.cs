using LiveDocs.Shared.Services;
using LiveDocs.WebApp.Data;
using LiveDocs.WebApp.Options;
using LiveDocs.WebApp.Services;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LiveDocs.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LiveDocsOptions>(Configuration.GetSection("LiveDocs"));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            MarkdownPipeline markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseBootstrap().Build();
            services.AddSingleton(markdownPipeline);
            services.AddSingleton<IDocumentationService, DocumentationService>();

            services.AddHostedService<ScheduledHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<LiveDocsOptions> liveDocsOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            StaticFileOptions staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(liveDocsOptions.Value.GetDocumentationFolderAsAbsolute(env.ContentRootPath).FullName),
                RequestPath = "/docs",
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            };

            app.UseStaticFiles(staticFileOptions);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
