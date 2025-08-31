using MediatR;
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
        private readonly IResultStore _resultStore;

        public GetUploadResultQueryHandler(IResultStore resultStore) => _resultStore = resultStore;

        public async Task<IDataResult<GetUploadResultQueryResult>> Handle(GetUploadResultQuery request, CancellationToken cancellationToken)
        {
            var (status, data) = await _resultStore.GetAsync(request.UploadId, cancellationToken);

            return status switch
            {
               ProcessingStatus.Completed when data is not null =>
                new SuccessDataResult<GetUploadResultQueryResult>(
                    new GetUploadResultQueryResult { Data = data },
                    "Upload processing completed successfully."
                ),

                ProcessingStatus.Pending =>
                new ErrorDataResult<GetUploadResultQueryResult>(
                    new GetUploadResultQueryResult(),
                    "Upload is still being processed."
                ),

                ProcessingStatus.NotFound =>
                    new ErrorDataResult<GetUploadResultQueryResult>(
                        new GetUploadResultQueryResult(),
                        "Upload not found."
                    ),

                _ =>
                    new ErrorDataResult<GetUploadResultQueryResult>(
                        new GetUploadResultQueryResult(),
                        "Unknown status."
                    )
            };
        }
    }
}
