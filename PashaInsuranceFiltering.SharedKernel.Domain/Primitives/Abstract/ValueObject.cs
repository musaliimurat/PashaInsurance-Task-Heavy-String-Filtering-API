using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType()) return false;

            if (ReferenceEquals(this, obj)) return true;

            var other = (ValueObject)obj;

            using var thisValues = GetEqualityComponents().GetEnumerator();
            using var otherValues = other.GetEqualityComponents().GetEnumerator();

            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (thisValues.Current is null ^ otherValues.Current is null) return false;
                if (thisValues.Current is not null && !thisValues.Current.Equals(otherValues.Current)) return false;
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var component in GetEqualityComponents())
                    hash = hash * 23 + (component?.GetHashCode() ?? 0);

                return hash;

            }
        }

        public static bool operator == (ValueObject? a, ValueObject? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            if (ReferenceEquals(a, b)) return true;
            return a.Equals(b);
        }

        public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
    }
}
