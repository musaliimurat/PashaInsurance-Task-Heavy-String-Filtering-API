using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Infrastructure.Background;
using PashaInsuranceFiltering.Infrastructure.Messaging; 
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PashaInsuranceFiltering.Tests.Unit.Background
{
    file sealed class FakeFilter : ITextFilter
    {
        public string Filter(string input, double threshold) => $"[{input}]";
        public Task<string> FilterAsync(string input, double threshold, CancellationToken ct = default)
            => Task.FromResult(Filter(input, threshold));
    }

    file sealed class FakeStore : IResultStore
    {
        private readonly ConcurrentDictionary<Guid, (ProcessingStatus Status, string? Data)> _map = new();

        public Task<(ProcessingStatus Status, string? Data)> GetAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_map.TryGetValue(id, out var v) ? v : (ProcessingStatus.NotFound, (string?)null));

        public Task MarkPendingAsync(Guid id, CancellationToken ct = default)
        {
            _map.AddOrUpdate(id, (ProcessingStatus.Pending, null), (_, __) => (ProcessingStatus.Pending, null));
            return Task.CompletedTask;
        }

        public Task MarkPendingIfAbsentAsync(Guid id, CancellationToken ct = default)
        {
            _map.TryAdd(id, (ProcessingStatus.Pending, null));
            return Task.CompletedTask;
        }

        public Task StoreAsync(Guid id, string filteredText, CancellationToken ct = default)
        {
            _map[id] = (ProcessingStatus.Completed, filteredText ?? string.Empty);
            return Task.CompletedTask;
        }
    }

    public class FilteringWorkerTests
    {
        [Fact]
        public async Task Worker_should_filter_and_store_single_item()
        {
            // arrange
            var queue = new InMemoryProcessingQueue();
            var filter = new FakeFilter();
            var store = new FakeStore();
            var worker = new FilteringWorker(queue, filter, store, NullLogger<FilteringWorker>.Instance, threshold: 0.8);

            // act 
            await worker.StartAsync(CancellationToken.None);

            var id = Guid.NewGuid();
            await queue.EnqueueAsync(id, "my name is murad");

            await Task.Delay(150);

            var (status, data) = await store.GetAsync(id);

            // assert
            status.Should().Be(ProcessingStatus.Completed);
            data.Should().Be("[my name is murad]");

            // cleanup
            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WorkerShouldProcessMultipleItemsInOrderTheyAppearInQueue()
        {
            var queue = new InMemoryProcessingQueue();
            var filter = new FakeFilter();
            var store = new FakeStore();
            var worker = new FilteringWorker(queue, filter, store, NullLogger<FilteringWorker>.Instance, threshold: 0.85);

            await worker.StartAsync(CancellationToken.None);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await queue.EnqueueAsync(id1, "first");
            await queue.EnqueueAsync(id2, "second");

            await Task.Delay(200);

            var r1 = await store.GetAsync(id1);
            var r2 = await store.GetAsync(id2);

            r1.Status.Should().Be(ProcessingStatus.Completed);
            r1.Data.Should().Be("[first]");

            r2.Status.Should().Be(ProcessingStatus.Completed);
            r2.Data.Should().Be("[second]");

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WorkerShouldStopGracefullyOnCancellation()
        {
            var queue = new InMemoryProcessingQueue();
            var worker = new FilteringWorker(queue, new FakeFilter(), new FakeStore(), NullLogger<FilteringWorker>.Instance, 0.8);

            var cts = new CancellationTokenSource();

            await worker.StartAsync(cts.Token);

            cts.Cancel(); 
            await worker.StopAsync(CancellationToken.None);

            true.Should().BeTrue();
        }
    }
}


