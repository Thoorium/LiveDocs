using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDocs.WebApp.Services.Queue
{
    public interface IBackgroundQueue<T>
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, T> workItem);

        Task<Func<CancellationToken, T>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
