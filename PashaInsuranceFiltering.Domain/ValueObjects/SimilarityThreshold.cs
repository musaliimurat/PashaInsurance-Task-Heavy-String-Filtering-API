using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Domain.ValueObjects
{
    public sealed class SimilarityThreshold : ValueObject
    {
        public double Value { get; }

        private SimilarityThreshold(double value) => Value = value; 

        public static SimilarityThreshold Create(double value)
        {
            if(value < 0 || value >1 || double.IsNaN(value))
                throw new DomainValidationException("Similarity threshold must be in range [0,1].");

            return new SimilarityThreshold(value);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString("F2");
       
    }
}
