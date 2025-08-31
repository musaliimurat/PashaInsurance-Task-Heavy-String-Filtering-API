using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.DomainEvents
{
    public interface IDomainEvent
    {
        DateTime OccurredOnUtc { get; }
    }
}
