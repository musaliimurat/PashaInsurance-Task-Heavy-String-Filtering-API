using PashaInsuranceFiltering.Application.Abstractions;
using PashaInsuranceFiltering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Infrastructure.Persistence.InMemory
{
    public sealed class InMemoryTextDocumentRepository : BaseInMemoryRepository<TextDocument, Guid>, ITextDocumentRepository
    {
        protected override Guid GetId(TextDocument entity) => entity.Id;
        
    }
}
