namespace PashaInsuranceFiltering.SharedKernel.Application.Result
{
    public class ErrorDataResult<T> : DataResult<T>
        where T : class, new()
    {
        public ErrorDataResult() : base(default, false)
        {
            
        }
        public ErrorDataResult(T data, string message) : base(data, false, message)
        {
        }
        public ErrorDataResult(string message) : base(default, false, message)
        {
            
        }
        public ErrorDataResult(T data) : base(data, false)
        {
        }
    }
}
