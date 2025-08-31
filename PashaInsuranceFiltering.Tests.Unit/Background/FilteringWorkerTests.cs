using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Domain.Entities;
using PashaInsuranceFiltering.Infrastructure.Background;
using PashaInsuranceFiltering.Infrastructure.Messaging;
using PashaInsuranceFiltering.Infrastructure.Persistence.InMemory;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PashaInsuranceFiltering.Tests.Unit.Background
{
    file static class TestHelpers
    {
        public static async Task WaitUntilAsync(Func<bool> cond, int timeoutMs = 2000, int stepMs = 20)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return;
                await Task.Delay(stepMs);
            }
            throw new TimeoutException("Condition not satisfied within timeout.");
        }
    }

    file sealed class FakeFilter : ITextFilter
    {
        public string Filter(string input, double threshold) => $"[{input}]";
        public Task<string> FilterAsync(string input, double threshold, CancellationToken ct = default)
            => Task.FromResult(Filter(input, threshold));
    }

    file sealed class FakeTextDocumentRepository : ITextDocumentRepository
    {
        private readonly ConcurrentDictionary<Guid, TextDocument> _store = new();

        public ConcurrentQueue<Guid> ProcessingLog { get; } = new();

        public Task AddAsync(TextDocument entity, CancellationToken ct)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task<TextDocument?> GetAsync(Guid id, CancellationToken ct)
        {
            _store.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(TextDocument entity, CancellationToken ct)
        {
            ProcessingLog.Enqueue(entity.Id);
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public bool TryGet(Guid id, out TextDocument? doc) => _store.TryGetValue(id, out doc);
        public TextDocument this[Guid id] => _store[id];
    }

    public class FilteringWorkerTests
    {

        [Fact]
        public async Task WorkerShouldFilterAndPersistSingleDocument()
        {
            // arrange
            IProcessingQueue queue = new InMemoryProcessingQueue();
            var repo = new FakeTextDocumentRepository();
            ITextFilter filter = new FakeFilter();
            var worker = new FilteringWorker(queue, repo, filter, NullLogger<FilteringWorker>.Instance, threshold: 0.8);

            await worker.StartAsync(CancellationToken.None);

            var id = Guid.NewGuid();
            var doc = new TextDocument(id, "my name is murad");
            await repo.AddAsync(doc, CancellationToken.None);

            // act
            await queue.EnqueueAsync(id, CancellationToken.None);

            // wait until processed
            await TestHelpers.WaitUntilAsync(() =>
                repo.TryGet(id, out var d) && d is not null && d.IsProcessed);

            // assert
            repo.TryGet(id, out var processed).Should().BeTrue();
            processed!.IsProcessed.Should().BeTrue();
            processed.FilteredText!.Value.Should().Be("[my name is murad]");

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WorkerShouldProcessMultipleItemsInQueueOrder()
        {
            // arrange
            IProcessingQueue queue = new InMemoryProcessingQueue();
            var repo = new FakeTextDocumentRepository();
            ITextFilter filter = new FakeFilter();
            var worker = new FilteringWorker(queue, repo, filter, NullLogger<FilteringWorker>.Instance, threshold: 0.8);

            await worker.StartAsync(CancellationToken.None);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await repo.AddAsync(new TextDocument(id1, "first"), CancellationToken.None);
            await repo.AddAsync(new TextDocument(id2, "second"), CancellationToken.None);

            // act
            await queue.EnqueueAsync(id1, CancellationToken.None);
            await queue.EnqueueAsync(id2, CancellationToken.None);

            // wait until both processed
            await TestHelpers.WaitUntilAsync(() =>
                repo.TryGet(id1, out var d1) && d1 is { IsProcessed: true } &&
                repo.TryGet(id2, out var d2) && d2 is { IsProcessed: true });

            // assert data
            repo[id1].FilteredText!.Value.Should().Be("[first]");
            repo[id2].FilteredText!.Value.Should().Be("[second]");

            repo.ProcessingLog.TryDequeue(out var p1).Should().BeTrue();
            repo.ProcessingLog.TryDequeue(out var p2).Should().BeTrue();
            p1.Should().Be(id1);
            p2.Should().Be(id2);

            await worker.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WorkerShouldStopGracefullyOnCancellation()
        {
            IProcessingQueue queue = new InMemoryProcessingQueue();
            var repo = new FakeTextDocumentRepository();
            ITextFilter filter = new FakeFilter();
            var worker = new FilteringWorker(queue, repo, filter, NullLogger<FilteringWorker>.Instance, threshold: 0.8);

            await worker.StartAsync(CancellationToken.None);
            await worker.StopAsync(CancellationToken.None);

            true.Should().BeTrue();
        }
    }
}


