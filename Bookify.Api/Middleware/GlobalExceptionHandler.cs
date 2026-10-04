using Bookify.Application.Abstractions.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bookify.Api.Middleware
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "Concurrency Conflict",
                    "The resource was modified by another request. Please retry."),

                DbUpdateException
                {
                    InnerException: PostgresException
                    {
                        SqlState: PostgresErrorCodes.UniqueViolation
                    }
                } => (
                    StatusCodes.Status409Conflict,
                    "Resource Already Exists",
                    "A resource with the same unique values already exists."),

                UserProfileIncompleteException => (
                    StatusCodes.Status400BadRequest,
                    "Incomplete Profile",
                    exception.Message),

                DbUpdateException
                {
                    InnerException: PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation }
                } => (
                    StatusCodes.Status409Conflict,
                    "Booking Overlap",
                    "The apartment is already booked for the selected dates."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Server Error",
                    "An unexpected error occurred.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning(exception, "Handled conflict ({Title}): {Message}", title, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Extensions = { ["traceId"] = httpContext.TraceIdentifier }
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);

            return true;
        }
    }
}