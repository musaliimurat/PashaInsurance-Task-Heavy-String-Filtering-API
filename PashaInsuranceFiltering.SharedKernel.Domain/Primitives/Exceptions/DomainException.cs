using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions
{
    public abstract class DomainException(string message) : Exception(message)
    {
    }

}
