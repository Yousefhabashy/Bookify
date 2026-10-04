using Bookify.Domain.Abstractions;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace Bookify.Application.Abstractions.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var errors = _validators
                .Select(validator => validator.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
                .ToArray();

            if (errors.Length == 0)
            {
                return await next();
            }

            var validationError = new ValidationError(errors);

            return CreateValidationResult<TResponse>(validationError);
        }

        private static TResult CreateValidationResult<TResult>(ValidationError validationError)
            where TResult : notnull
        {
            if (typeof(TResult) == typeof(Result))
            {
                return (TResult)(object)Result.Failure(validationError);
            }

            if (typeof(TResult).IsGenericType &&
                typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = typeof(TResult).GetGenericArguments()[0];

                MethodInfo failureMethod = typeof(Result)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                    .MakeGenericMethod(innerType);

                return (TResult)failureMethod.Invoke(null, new object[] { validationError })!;
            }

            throw new InvalidOperationException(
                $"Cannot create validation result for type {typeof(TResult).Name}");
        }
    }
}