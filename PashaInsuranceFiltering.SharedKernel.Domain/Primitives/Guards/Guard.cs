using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Guards
{
    public static class Guard
    {
        public static Guid NotEmpty(Guid id, string name) 
            =>  id == Guid.Empty ? throw new DomainValidationException($"{name} cannot be empty.") : id;

        public static string NotNullOrWhiteSpace(string value, string name)
            => string.IsNullOrWhiteSpace(value) ? throw new DomainValidationException($"{name} cannot be null or whitespace.") : value!;
    }
}
