using PashaInsuranceFiltering.Application.Common.Ports;
using System.Collections.Concurrent;

namespace PashaInsuranceFiltering.Infrastructure.Persistence.InMemory
{
    public sealed class InMemoryResultStore : IResultStore
    {
        private readonly ConcurrentDictionary<Guid, (ProcessingStatus Status, string? Data)> _store = new();

        public Task<(ProcessingStatus Status, string? Data)> GetAsync(Guid uploadId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.TryGetValue(uploadId, out var result) 
                ? result
                : (ProcessingStatus.NotFound, null));
        }

        public Task MarkPendingAsync(Guid uploadId, CancellationToken ct = default)
        {
            _store.AddOrUpdate(uploadId, (ProcessingStatus.Pending, null), (_, __) => (ProcessingStatus.Pending, null));
            return Task.CompletedTask;
        }

        public Task StoreAsync(Guid uploadId, string filteredText, CancellationToken ct = default)
        {
            _store[uploadId] = (ProcessingStatus.Completed, filteredText ?? string.Empty);
            return Task.CompletedTask;
        }
    }
}
