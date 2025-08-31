namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions
{
    public sealed class DomainValidationException(string message) : DomainException(message)
    {
    }

}
