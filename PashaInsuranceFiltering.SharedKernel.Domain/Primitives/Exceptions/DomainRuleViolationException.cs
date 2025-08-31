namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions
{
    public class DomainRuleViolationException(string message) : DomainException(message)
    {
    }

}
