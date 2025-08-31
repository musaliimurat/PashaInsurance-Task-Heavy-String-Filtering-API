using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Infrastructure.Messaging;

namespace PashaInsuranceFiltering.Infrastructure.Background
{

public sealed class FilteringWorker : BackgroundService
{
        private readonly IProcessingQueue _queue;
        private readonly ITextFilter _filter;
        private readonly IResultStore _store;
        private readonly ILogger<FilteringWorker> _logger;
        private readonly double _threshold;

        public FilteringWorker(
            IProcessingQueue queue,
            ITextFilter filter,
            IResultStore store,
            ILogger<FilteringWorker> logger,
            double threshold = 0.8)
        {
        _queue = queue;
        _filter = filter;
        _store = store;
        _logger = logger;
        _threshold = Math.Clamp(threshold, 0.0, 1.0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (uploadId, fullText) in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                var filtered = await _filter.FilterAsync(fullText, _threshold, stoppingToken);
                await _store.StoreAsync(uploadId, filtered, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtering failed for UploadId {UploadId}", uploadId);
            }
        }
    }
}
}
