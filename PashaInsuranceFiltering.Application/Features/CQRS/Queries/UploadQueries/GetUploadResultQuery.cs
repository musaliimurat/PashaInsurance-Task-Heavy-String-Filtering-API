using MediatR;
using PashaInsuranceFiltering.Application.Features.CQRS.Results.UploadResults;
using PashaInsuranceFiltering.SharedKernel.Application.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Features.CQRS.Queries.UploadQueries
{
    public sealed record GetUploadResultQuery(Guid UploadId) : IRequest<IDataResult<GetUploadResultQueryResult>>;
    

}
