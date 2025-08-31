using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Infrastructure.Messaging;

namespace PashaInsuranceFiltering.Infrastructure.Background
{

public sealed class FilteringWorker : BackgroundService
{
        private readonly IProcessingQueue _queue;
        private readonly ITextFilter _filter;
        private readonly ITextDocumentRepository _repo;
        private readonly ILogger<FilteringWorker> _logger;
        private readonly double _threshold;

        public FilteringWorker(
            IProcessingQueue queue,
            ITextDocumentRepository repo,
            ITextFilter filter,
            ILogger<FilteringWorker> logger,
            double threshold = 0.8
              )
        {
            _queue = queue;
            _repo = repo;
            _filter = filter;
            _logger = logger;
            _threshold = Math.Clamp(threshold, 0.0, 1.0);
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
    {
            while (!ct.IsCancellationRequested)
            {
                var id = await _queue.DequeueAsync(ct);
                var doc = await _repo.GetAsync(id, ct);
                if (doc is null) continue;

                var filtered = await _filter.FilterAsync(doc.OriginalText, _threshold, ct);

                doc.ApplyFiltering(filtered, _threshold);
                await _repo.UpdateAsync(doc, ct);
            }
        }
}
}
