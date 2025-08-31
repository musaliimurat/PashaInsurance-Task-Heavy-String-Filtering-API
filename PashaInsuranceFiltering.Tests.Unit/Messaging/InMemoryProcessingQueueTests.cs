using FluentAssertions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Infrastructure.Messaging;
using System.Diagnostics;
using Xunit;

namespace PashaInsuranceFiltering.Tests.Unit.Messaging
{
    public class InMemoryProcessingQueueTests
    {
        [Fact]
        public async Task EnqueueThenDequeueShouldReturnSameId()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();
            var id = Guid.NewGuid();

            await q.EnqueueAsync(id, CancellationToken.None);
            var got = await q.DequeueAsync(CancellationToken.None);

            got.Should().Be(id);
        }

        [Fact]
        public async Task ShouldBlockUntilItemIsEnqueuedThenResume()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            var dequeueTask = q.DequeueAsync(cts.Token);

            await Task.Delay(100);
            dequeueTask.IsCompleted.Should().BeFalse();

            var id = Guid.NewGuid();
            await q.EnqueueAsync(id, CancellationToken.None);

            var got = await dequeueTask; 
            got.Should().Be(id);
        }

        [Fact]
        public async Task DequeueShouldBeFIF0ForMultipleItems()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            await q.EnqueueAsync(id1, CancellationToken.None);
            await q.EnqueueAsync(id2, CancellationToken.None);
            await q.EnqueueAsync(id3, CancellationToken.None);

            (await q.DequeueAsync(CancellationToken.None)).Should().Be(id1);
            (await q.DequeueAsync(CancellationToken.None)).Should().Be(id2);
            (await q.DequeueAsync(CancellationToken.None)).Should().Be(id3);
        }

        [Fact]
        public async Task DequeueShouldHonorCancellation()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();
            using var cts = new CancellationTokenSource();

            var dequeueTask = q.DequeueAsync(cts.Token);

            cts.CancelAfter(100);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await dequeueTask);
        }

        [Fact]
        public async Task ConcurrentProducersAndSingleConsumerPreserveOrderPerQueueing()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();

            var total = 200;
            var ids = Enumerable.Range(0, total).Select(_ => Guid.NewGuid()).ToArray();

            var p1 = Task.Run(async () =>
            {
                for (int i = 0; i < total; i += 2)
                    await q.EnqueueAsync(ids[i], CancellationToken.None);
            });

            var p2 = Task.Run(async () =>
            {
                for (int i = 1; i < total; i += 2)
                    await q.EnqueueAsync(ids[i], CancellationToken.None);
            });

            await Task.WhenAll(p1, p2);

            // single consumer
            var received = new List<Guid>(total);
            for (int i = 0; i < total; i++)
                received.Add(await q.DequeueAsync(CancellationToken.None));

            received.Should().HaveCount(total);
            received.Should().BeEquivalentTo(ids); 

            var evens = ids.Where((_, i) => i % 2 == 0).ToArray();
            var odds = ids.Where((_, i) => i % 2 == 1).ToArray();

            int lastEvenPos = -1;
            foreach (var id in evens)
            {
                var pos = received.IndexOf(id);
                pos.Should().BeGreaterThan(lastEvenPos); 
                lastEvenPos = pos;
            }

            int lastOddPos = -1;
            foreach (var id in odds)
            {
                var pos = received.IndexOf(id);
                pos.Should().BeGreaterThan(lastOddPos);
                lastOddPos = pos;
            }
        }

        [Fact]
        public async Task StressItemsShouldAllBeDequeued()
        {
            IProcessingQueue q = new InMemoryProcessingQueue();

            var count = 1000;
            var ids = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();

            foreach (var id in ids)
                await q.EnqueueAsync(id, CancellationToken.None);

            var got = new List<Guid>(count);
            for (int i = 0; i < count; i++)
                got.Add(await q.DequeueAsync(CancellationToken.None));

            got.Should().BeEquivalentTo(ids, o => o.WithStrictOrdering());
        }
    }
}
