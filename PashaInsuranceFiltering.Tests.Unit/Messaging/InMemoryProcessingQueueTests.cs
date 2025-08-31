using FluentAssertions;
using PashaInsuranceFiltering.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Tests.Unit.Messaging
{
    public class InMemoryProcessingQueueTests
    {
        [Fact]
        public async Task EnqueueThenDequeueShouldPreserveOrderForSameUpload()
        {
            var queue = new InMemoryProcessingQueue();
            var id = Guid.NewGuid();

            await queue.EnqueueAsync(id, "First");
            await queue.EnqueueAsync(id, "Second");
            await queue.EnqueueAsync(id, "Third");

            var cts = new CancellationTokenSource();
            var enumerator = queue.DequeueAllAsync(cts.Token).GetAsyncEnumerator();

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.uploadId.Should().Be(id);
            enumerator.Current.fullText.Should().Be("First");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.fullText.Should().Be("Second");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.fullText.Should().Be("Third");

            cts.Cancel();
        }

        [Fact]
        public async Task QueueShouldHandleMultipleUploadIdsInterleaved()
        {
            var queue = new InMemoryProcessingQueue();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await queue.EnqueueAsync(id1, "FirstA");
            await queue.EnqueueAsync(id2, "FirstB");
            await queue.EnqueueAsync(id1, "SecondA");
            await queue.EnqueueAsync(id2, "SecondB");


            var cts = new CancellationTokenSource();
            var enumerator = queue.DequeueAllAsync(cts.Token).GetAsyncEnumerator();

            var seen = new List<(Guid id, string text)>();
            for (int i = 0; i < 4; i++)
            {
                (await enumerator.MoveNextAsync()).Should().BeTrue();
                seen.Add((enumerator.Current.uploadId, enumerator.Current.fullText));
            }

            seen.Should().ContainInOrder(
                (id1, "FirstA"),
                (id2, "FirstB"),
                (id1, "SecondA"),
                (id2, "SecondB")
            );

            cts.Cancel();
        }

        [Fact]
        public async Task DequeueAllAsyncShouldStopWhenCancelled()
        {
            var queue = new InMemoryProcessingQueue();
            var id = Guid.NewGuid();

            await queue.EnqueueAsync(id, "OnlyItem");

            var cts = new CancellationTokenSource();
            var enumerator = queue.DequeueAllAsync(cts.Token).GetAsyncEnumerator();

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.fullText.Should().Be("OnlyItem");

            cts.Cancel();

            Func<Task> act = async () =>
            {
                var moved = await enumerator.MoveNextAsync();
                if (moved) throw new Exception("Enumerator should not continue after cancellation.");

            };

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task EnqueueFromMultipleTasksThenDequeueShouldYieldAllItems()
        {
            var q = new InMemoryProcessingQueue();
            var id = Guid.NewGuid();

            // 10 concurrent enqueues
            var tasks = Enumerable.Range(1, 10)
                .Select(i => q.EnqueueAsync(id, $"P{i}"))
                .ToArray();

            await Task.WhenAll(tasks);

            var cts = new CancellationTokenSource();
            var enumerator = q.DequeueAllAsync(cts.Token).GetAsyncEnumerator();

            var results = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                (await enumerator.MoveNextAsync()).Should().BeTrue();
                results.Add(enumerator.Current.fullText);
            }

            results.Should().HaveCount(10);

            //FIFO
            results.Should().ContainInOrder(Enumerable.Range(1, 10).Select(i => $"P{i}"));

            
            //results.Should().BeEquivalentTo(Enumerable.Range(1, 10).Select(i => $"P{i}"));

            cts.Cancel();
        }
    }
}
