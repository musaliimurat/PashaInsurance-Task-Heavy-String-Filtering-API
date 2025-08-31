using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Common.Ports
{
    public interface IProcessingQueue
    {
        Task EnqueueAsync(Guid uploadId, CancellationToken ct = default);
        Task<Guid> DequeueAsync(CancellationToken ct = default);
    }
}
