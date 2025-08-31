using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Common.Ports
{
    public enum ProcessingStatus
    {
        NotFound = 0,
        Pending = 1,
        Completed = 2,
    }
    public interface IResultStore
    {
        Task MarkPendingAsync(Guid uploadId, CancellationToken ct = default);
        Task StoreAsync(Guid uploadId, string filteredText, CancellationToken ct = default);
        Task<(ProcessingStatus Status, string? Data)> GetAsync(Guid uploadId, CancellationToken cancellationToken = default);
    }
}
