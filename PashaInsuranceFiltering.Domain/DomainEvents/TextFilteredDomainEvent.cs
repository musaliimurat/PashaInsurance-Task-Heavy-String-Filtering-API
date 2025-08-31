using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.DomainEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Domain.DomainEvents
{
    public sealed class TextFilteredDomainEvent(Guid documentId) : DomainEventBase
    {
        public Guid DocumentId { get;} = documentId;
       
    }
}
