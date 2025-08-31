using FluentValidation;
using MediatR;
using PashaInsuranceFiltering.SharedKernel.Application.Result;
using System.Reflection;

namespace PashaInsuranceFiltering.SharedKernel.Application.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToArray();

            if (failures.Length == 0)
                return await next();

            
            var message = string.Join(" | ", failures.Select(f => f.ErrorMessage));

           
            if (typeof(TResponse) == typeof(Result.Result))
            {
                return (TResponse)(object)new ErrorResult(message);
            }

            var tResp = typeof(TResponse);

            if (tResp.IsInterface && tResp.IsGenericType && tResp.GetGenericTypeDefinition().Name is "IDataResult`1")
            {
                var dataType = tResp.GetGenericArguments()[0];                   
                var errType = typeof(ErrorDataResult<>).MakeGenericType(dataType);

                var instance = Activator.CreateInstance(
                    errType,
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    args: new object?[] { null, message },
                    culture: null);

                return (TResponse)instance!;
            }

            throw new ValidationException(failures);
        }
    }
}
