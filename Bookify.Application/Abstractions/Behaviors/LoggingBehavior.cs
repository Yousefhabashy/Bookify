using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Abstractions.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var name = request.GetType().Name;

            _logger.LogInformation("Executing request {Request}", name);

            var result = await next();

            if (result is Result { IsFailure: true } failureResult)
            {
                _logger.LogWarning(
                    "Request {Request} completed with failure: {@Error}",
                    name,
                    failureResult.Error);
            }
            else
            {
                _logger.LogInformation("Request {Request} processed successfully", name);
            }

            return result;
        }
    }
}