using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using PashaInsuranceFiltering.SharedKernel.Application.Result;

namespace PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands
{
    public sealed record UploadChunkCommand(
          Guid UploadId,
        int ChunkIndex,
        string Data,
        bool IsLastChunk
        ) : IRequest<IResult>;
  
}
