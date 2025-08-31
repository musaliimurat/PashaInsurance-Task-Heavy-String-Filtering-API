using FluentAssertions;
using PashaInsuranceFiltering.Infrastructure.Persistence.InMemory;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions;
using System.Text;
using Xunit;

namespace PashaInsuranceFiltering.Tests.Unit.Persistence
{
    public class InMemoryUploadBufferTests
    {
        [Fact]
        public async Task AddChunkAsyncShouldCombineChunksWhenLastChunk()
        {
            var buffer = new InMemoryUploadBuffer();
            var uploadId = Guid.NewGuid();

            var part1 = await buffer.AddChunkAsync(uploadId, 0, "My name ", false);
            part1.Should().BeNull();

            var part2 = await buffer.AddChunkAsync(uploadId, 1, "is Murad", true);

            part2.Should().Be("My name is Murad");
        }

        [Fact]
        public async Task AddChunkAsyncShouldReturnNullWhenNotLastChunk()
        {
            var buffer = new InMemoryUploadBuffer();
            var uploadId = Guid.NewGuid();

            var part = await buffer.AddChunkAsync(uploadId, 0, "Some text", false);

            part.Should().BeNull();
        }

        [Fact]
        public async Task AddChunkAsyncShouldThrowWhenExceeding()
        {
            var buffer = new InMemoryUploadBuffer();
            var uploadId = Guid.NewGuid();

            var bigData = new string('m', 101 * 1024 * 1024);

            Func<Task> act = async () =>
            {
                await buffer.AddChunkAsync(uploadId, 0, bigData, true);
            };

            await act.Should().ThrowAsync<DomainValidationException>();
        }

        [Fact]
        public async Task AddChunkAsyncShouldRespectChunkOrder()
        {
            var buffer = new InMemoryUploadBuffer();
            var uploadId = Guid.NewGuid();

            // before chunk 1
            await buffer.AddChunkAsync(uploadId, 1, "is Murad", false);
            // after chunk 0
            await buffer.AddChunkAsync(uploadId, 0, "My name ", false);

            // end chunk 2
            var result = await buffer.AddChunkAsync(uploadId, 2, "!", true);

            result.Should().Be("My name is Murad!");
        }
    }

}

