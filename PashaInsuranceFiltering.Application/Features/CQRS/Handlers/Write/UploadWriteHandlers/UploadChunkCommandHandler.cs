using MediatR;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using PashaInsuranceFiltering.SharedKernel.Application.Result;

namespace PashaInsuranceFiltering.Application.Features.CQRS.Handlers.Write.UploadWriteHandlers
{
    public sealed class UploadChunkCommandHandler : IRequestHandler<UploadChunkCommand, IResult>
    {
        private readonly IUploadBuffer _uploadBuffer;
        private readonly IProcessingQueue _processingQueue;
        private readonly IResultStore _resultStore;

        public UploadChunkCommandHandler(IUploadBuffer uploadBuffer, IProcessingQueue processingQueue, IResultStore resultStore)
        {
            _uploadBuffer = uploadBuffer;
            _processingQueue = processingQueue;
            _resultStore = resultStore;
        }

        public async Task<IResult> Handle(UploadChunkCommand request, CancellationToken cancellationToken)
        {
            await _resultStore.MarkPendingAsync(request.UploadId, cancellationToken);

            var full = await _uploadBuffer.AddChunkAsync(
                request.UploadId, request.ChunkIndex, request.Data, request.IsLastChunk, cancellationToken);

            if (full is not null)
            {
                await _processingQueue.EnqueueAsync(request.UploadId, full, cancellationToken);
            }

            return new SuccessResult("Accepted");
        }
    }
}
