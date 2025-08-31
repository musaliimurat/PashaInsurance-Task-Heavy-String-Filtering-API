using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.DomainEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract
{
    public abstract class AggregateRoot<TId> : Entity<TId>
        where TId : notnull
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot() { }
        protected AggregateRoot(TId id) : base(id) { }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void RaiseDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
