using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Domain.ValueObjects
{
    public sealed class FilteredText : ValueObject
    {
        public string Value { get; }

        private FilteredText(string value) => Value = value;

        public static FilteredText Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainValidationException("Filtered text cannot be null or whitespace.");

            return new FilteredText(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

    }
}
