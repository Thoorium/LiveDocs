using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace LiveDocs.Shared.Services.Remote
{
    public class RemoteDocumentationDocument
    {
        public DateTime LastUpdate { get; set; }
        public string Path { get; set; }
        public RemoteDocumentationDocument[] SubDocuments { get; set; }

        [JsonIgnore]
        public int SubDocumentsCount => (SubDocuments?.Length ?? 0) + SubDocuments?.Sum(s => s.SubDocumentsCount) ?? 0;
    }
}