using System.Collections.Concurrent;
using System.Text;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions;

namespace PashaInsuranceFiltering.Infrastructure.Persistence.InMemory
{
    public sealed class InMemoryUploadBuffer : IUploadBuffer
    {
        private sealed class UploadState
        {
            public ConcurrentDictionary<int, string> Chunks { get; } = new();
            public long TotalBytes; // UTF8 byte count of all current chunks
        }

        private readonly ConcurrentDictionary<Guid, UploadState> _uploads = new();

        public const long MaxTotalBytes = 100L * 1024 * 1024; // 100 MB

        public Task<string?> AddChunkAsync(
            Guid uploadId,
            int chunkIndex,
            string data,
            bool isLastChunk,
            CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested) return Task.FromCanceled<string?>(ct);
            if (chunkIndex < 0) throw new DomainValidationException("chunkIndex must be non-negative.");

            var state = _uploads.GetOrAdd(uploadId, _ => new UploadState());

            var newBytes = Encoding.UTF8.GetByteCount(data ?? string.Empty);
            var oldBytes = 0;

            if (state.Chunks.TryGetValue(chunkIndex, out var old))
                oldBytes = Encoding.UTF8.GetByteCount(old);

            // write new value first so dictionary has the latest data
            state.Chunks[chunkIndex] = data ?? string.Empty;

            var delta = newBytes - oldBytes;
            var after = Interlocked.Add(ref state.TotalBytes, delta);

            if (after > MaxTotalBytes)
            {
                // clean up this upload to free memory, then fail
                _uploads.TryRemove(uploadId, out _);
                throw new DomainValidationException(
                    $"Upload {uploadId} exceeded max size of {MaxTotalBytes} bytes.");
            }

            // if not last chunk, nothing to return yet
            if (!isLastChunk) return Task.FromResult<string?>(null);

            // last chunk  - concatenate in order and clear state
            var ordered = state.Chunks.OrderBy(kv => kv.Key).Select(kv => kv.Value);
            var fullData = string.Concat(ordered);

            _uploads.TryRemove(uploadId, out _); 

            return Task.FromResult<string?>(fullData);
        }
    }
}
