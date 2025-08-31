namespace PashaInsuranceFiltering.SharedKernel.Application.Result
{
    public class SuccessDataResult<T> : DataResult<T>
        where T : class, new()
    {
        public SuccessDataResult(T data, string message) : base(data, true, message)
        {
        }
        public SuccessDataResult(T data) : base(data, true)
        {
        }
      
    }
}
