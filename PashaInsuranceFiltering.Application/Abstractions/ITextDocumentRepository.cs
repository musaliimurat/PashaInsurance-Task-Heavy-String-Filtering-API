using PashaInsuranceFiltering.Domain.Entities;
using PashaInsuranceFiltering.SharedKernel.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Application.Abstractions
{
    public interface ITextDocumentRepository : IRepositoryBase<TextDocument, Guid>
    {
    }
}
