namespace PashaInsuranceFiltering.SharedKernel.Application.Result
{
    public interface IDataResult<T> : IResult
        where T : class, new()
    {
        T Data { get; }
    }
}
