using FluentValidation;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Features.Validators.UploadValidators
{
    public sealed class UploadChunkCommandValidator : AbstractValidator<UploadChunkCommand>
    {
        public UploadChunkCommandValidator()
        {
            RuleFor(x => x.UploadId)
                .NotEmpty().WithMessage("UploadId cannot be an empty GUID.");

            RuleFor(x => x.ChunkIndex)
                .GreaterThanOrEqualTo(0).WithMessage("ChunkIndex must be a non-negative integer.");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data cannot be null or empty.");
        }
    }
}
