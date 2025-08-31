using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Common.Ports
{
    public interface IUploadBuffer
    {
        Task<string?> AddChunkAsync(Guid uploadId, int chunkIndex, string data, bool isLastChunk, CancellationToken ct = default);
    }
}
