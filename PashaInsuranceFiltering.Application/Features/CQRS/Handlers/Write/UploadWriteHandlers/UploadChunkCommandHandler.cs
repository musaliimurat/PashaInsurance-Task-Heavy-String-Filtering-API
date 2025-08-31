using MediatR;
using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using PashaInsuranceFiltering.Domain.Entities;
using PashaInsuranceFiltering.SharedKernel.Application.Result;

namespace PashaInsuranceFiltering.Application.Features.CQRS.Handlers.Write.UploadWriteHandlers
{
    public sealed class UploadChunkCommandHandler : IRequestHandler<UploadChunkCommand, IResult>
    {
        private readonly IUploadBuffer _uploadBuffer;
        private readonly IProcessingQueue _processingQueue;
        private readonly ITextDocumentRepository _textDocumentRepository;

        public UploadChunkCommandHandler(IUploadBuffer uploadBuffer, IProcessingQueue processingQueue, ITextDocumentRepository textDocumentRepository)
        {
            _uploadBuffer = uploadBuffer;
            _processingQueue = processingQueue;
            _textDocumentRepository = textDocumentRepository;
        }

        public async Task<IResult> Handle(UploadChunkCommand request, CancellationToken cancellationToken)
        {
            var full = await _uploadBuffer.AddChunkAsync(
                request.UploadId, request.ChunkIndex, request.Data, request.IsLastChunk, cancellationToken);

            if (full is not null)
            {
                var doc = new TextDocument(request.UploadId, full);
                await _textDocumentRepository.AddAsync(doc, cancellationToken);

                await _processingQueue.EnqueueAsync(request.UploadId, cancellationToken);
            }

            return new SuccessResult("Accepted");
        }
    }
}
