namespace PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Exceptions
{
    public sealed class NotFoundException(string entity, object key) : DomainException($"Entity '{entity}' with key '{key}' was not found.")
    {
    }

}
