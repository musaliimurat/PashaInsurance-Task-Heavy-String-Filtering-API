namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions
{
    public sealed class ConcurrencyConflictException(string message) : DomainException(message)
    {
    }

}
