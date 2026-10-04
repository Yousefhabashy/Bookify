using Bookify.Application.Reviews.CreateReview;
using Bookify.Application.Reviews.GetReviewById;
using Bookify.Application.Reviews.GetReviewsByApartmentId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Reviews
{
    [Route("api/reviews")]
    public class ReviewsController : ApiController
    {
        public ReviewsController(ISender sender) : base(sender)
        {
        }

        [HttpGet("apartment/{apartmentId:guid}")]
        public async Task<IActionResult> GetReviewsByApartmentId(Guid apartmentId, CancellationToken cancellationToken)
        {
            var query = new GetReviewsByApartmentIdQuery(apartmentId);

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReview(
            [FromBody] CreateReviewRequest request,
            CancellationToken cancellationToken
            )
        {
            var command = new CreateReviewCommand(request.BookingId, request.Rating, request.Comment);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value) : HandleFailure(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetReviewByIdQuery(id);

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }
    }
}
