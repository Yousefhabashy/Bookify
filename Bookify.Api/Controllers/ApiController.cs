using Bookify.Application.Abstractions;
using Bookify.Domain.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers
{
    [ApiController]
    public abstract class ApiController : ControllerBase
    {
        protected readonly ISender Sender;

        protected ApiController(ISender sender)
        {
            Sender = sender;
        }

        protected IActionResult HandleFailure(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Cannot handle failure for a successful result.");
            }

            return result.Error switch
            {
                ValidationError validationError => BadRequest(CreateProblemDetails(
                    "Validation Error", StatusCodes.Status400BadRequest, validationError, validationError.Errors)),

                { Type: ErrorType.NotFound } => NotFound(CreateProblemDetails(
                    "Not Found", StatusCodes.Status404NotFound, result.Error)),

                { Type: ErrorType.Conflict } => Conflict(CreateProblemDetails(
                    "Conflict", StatusCodes.Status409Conflict, result.Error)),

                { Type: ErrorType.Forbidden } => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails(
                    "Forbidden", StatusCodes.Status403Forbidden, result.Error)),


                _ => BadRequest(CreateProblemDetails(
                    "Bad Request", StatusCodes.Status400BadRequest, result.Error))
            };
        }

        private static ProblemDetails CreateProblemDetails(
            string title, int status, Error error, Error[]? errors = null)
        {
            return new ProblemDetails
            {
                Title = title,
                Type = error.Code,
                Detail = error.Message,
                Status = status,
                Extensions = { { "errors", errors } }
            };
        }
    }
}
