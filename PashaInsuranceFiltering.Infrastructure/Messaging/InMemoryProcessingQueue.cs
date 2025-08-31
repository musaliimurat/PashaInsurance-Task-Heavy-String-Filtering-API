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
        private readonly ConcurrentQueue<Guid> _q = new();
        private readonly SemaphoreSlim _signal = new(0);

       
        public Task EnqueueAsync(Guid uploadId, CancellationToken ct = default)
        {
            _q.Enqueue(uploadId);
            _signal.Release();
            return Task.CompletedTask;
        }

        public async Task<Guid> DequeueAsync(CancellationToken ct = default)
        {
            await _signal.WaitAsync(ct);
            if (_q.TryDequeue(out var id)) return id;
            throw new InvalidOperationException("Queue signaled but empty.");
        }

        public void Dispose() => _signal.Dispose();
      
    }
}
