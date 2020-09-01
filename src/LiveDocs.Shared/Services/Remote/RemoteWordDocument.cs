using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using LiveDocs.Shared.Services.Documents;
using Mammoth;
using Microsoft.Extensions.DependencyInjection;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteWordDocument : WordDocument, IRemoteDocumentationDocument
    {
        private readonly IServiceProvider _Services;
        private DocumentConverter converter;
        private byte[] data;
        private bool readingCache;

        public RemoteWordDocument(IServiceProvider serviceProvider)
        {
            _Services = serviceProvider;
            converter = new DocumentConverter().AddStyleMap("table => table.table:fresh").ImageConverter(image =>
            {
                using (var stream = image.GetStream())
                {
                    var base64 = StreamToBase64(stream);
                    var src = "data:" + image.ContentType + ";base64," + base64;
                    return new Dictionary<string, string> {
                        { "src", src },
                        //{ "alt", image.AltText },
                        { "class", "img-fluid" }
                    };
                }
            });
        }

        public override byte[] Data => data;
        protected override DocumentConverter wordConverter => converter;
        public async Task<bool> TryCache()
        {
            if (Data != null)
                return true;

            while (readingCache)
                await Task.Delay(5);

            if (Data != null)
                return true;

            readingCache = true;
            try
            {
                using (var scope = _Services.CreateScope())
                {
                    var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                    data = await httpClient.GetByteArrayAsync(Path);
                }
            } catch
            {
                return false;
            } finally
            {
                readingCache = false;
            }

            return true;
        }

        public async Task<(bool, string)> TryToHtml(IDocumentationProject documentationProject, string baseUri)
        {
            var cacheResult = await TryCache();
            string htmlString = "";
            if (cacheResult)
                htmlString = await ToHtml(documentationProject, baseUri);

            return (cacheResult, htmlString);
        }

        private string StreamToBase64(Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return Convert.ToBase64String(bytes);
        }
    }
}