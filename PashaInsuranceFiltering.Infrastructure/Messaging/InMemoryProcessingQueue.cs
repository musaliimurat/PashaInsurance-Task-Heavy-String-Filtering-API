using PashaInsuranceFiltering.Application.Common.Ports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Infrastructure.Messaging
{
    public sealed class InMemoryProcessingQueue : IProcessingQueue, IDisposable
    {
        private readonly ConcurrentQueue<(Guid uploadId, string fullText)> _q = new();
        private readonly SemaphoreSlim _signal = new(0);

        public Task EnqueueAsync(Guid uploadId, string fullText, CancellationToken ct = default)
        {
            _q.Enqueue((uploadId, fullText));
            _signal.Release();
            return Task.CompletedTask;
        }

        public async IAsyncEnumerable<(Guid uploadId, string fullText)> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                await _signal.WaitAsync(ct);
                while (_q.TryDequeue(out var item))
                    yield return item;
            }
        }

        public void Dispose() => _signal.Dispose();
    }
}
