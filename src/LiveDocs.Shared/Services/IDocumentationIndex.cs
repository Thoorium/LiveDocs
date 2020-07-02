using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services
{
    public interface IDocumentationIndex
    {
        List<IDocumentationDocument> Documents { get; set; }
    }
}
