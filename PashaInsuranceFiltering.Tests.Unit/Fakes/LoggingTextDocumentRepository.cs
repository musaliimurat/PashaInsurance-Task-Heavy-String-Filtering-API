using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Tests.Unit.Fakes
{
    public sealed class LoggingTextDocumentRepository : ITextDocumentRepository
    {
        private readonly ITextDocumentRepository _inner;
        public ConcurrentQueue<Guid> ProcessingLog { get; } = new();

        public LoggingTextDocumentRepository(ITextDocumentRepository inner) => _inner = inner;

        public Task AddAsync(TextDocument e, CancellationToken ct) => _inner.AddAsync(e, ct);

        public Task<TextDocument?> GetAsync(Guid id, CancellationToken ct) => _inner.GetAsync(id, ct);

        public async Task UpdateAsync(TextDocument e, CancellationToken ct)
        {
            ProcessingLog.Enqueue(e.Id);
            await _inner.UpdateAsync(e, ct);
        }
    }

}
