using MediatR;
using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Application.Common.Ports;
using PashaInsuranceFiltering.Application.Features.CQRS.Queries.UploadQueries;
using PashaInsuranceFiltering.Application.Features.CQRS.Results.UploadResults;
using PashaInsuranceFiltering.SharedKernel.Application.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Features.CQRS.Handlers.Read.UploadReadHandlers
{
    public sealed class GetUploadResultQueryHandler : IRequestHandler<GetUploadResultQuery, IDataResult<GetUploadResultQueryResult>>
    {
        private readonly ITextDocumentRepository _repo;

        public GetUploadResultQueryHandler(ITextDocumentRepository repo) => _repo = repo;

        public async Task<IDataResult<GetUploadResultQueryResult>> Handle(GetUploadResultQuery request, CancellationToken cancellationToken)
        {
            var doc = await _repo.GetAsync(request.UploadId, cancellationToken);

            if (doc is null)
                return new ErrorDataResult<GetUploadResultQueryResult>("Not Found");

            if (!doc.IsProcessed)
                return new ErrorDataResult<GetUploadResultQueryResult>("Processing");

            var result = new GetUploadResultQueryResult
            {
               Data = doc.FilteredText?.Value ?? string.Empty
            };

            return new SuccessDataResult<GetUploadResultQueryResult>(result, "Completed");
        }
    }
}
