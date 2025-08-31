﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract
{
    public abstract class Entity<TId>
        where TId : notnull
    {
        public TId Id { get; protected set; }

        public DateTime CreatedAtUtc { get; protected set; }
        public DateTime? ModifiedAtUtc { get; protected set; }

        protected Entity() => Id = default!;

        protected Entity(TId id) => Id = id;

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TId> other) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => HashCode.Combine(GetType(), Id);

        public void MarkAsCreated() => CreatedAtUtc = DateTime.UtcNow;
        public void MarkAsModified() => ModifiedAtUtc = DateTime.UtcNow;

    }
}
