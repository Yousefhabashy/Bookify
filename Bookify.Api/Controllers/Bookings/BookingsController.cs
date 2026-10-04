using Bookify.Application.Bookings.CancelBooking;
using Bookify.Application.Bookings.CompleteBooking;
using Bookify.Application.Bookings.ConfirmBooking;
using Bookify.Application.Bookings.GetBookingById;
using Bookify.Application.Bookings.GetBookingsByUserId;
using Bookify.Application.Bookings.RejectBooking;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Domain.Bookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Bookings
{
    [Route("api/bookings")]
    public class BookingsController : ApiController
    {
        public BookingsController(ISender sender) : base(sender)
        {
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetBookingByIdQuery(id);

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyBookings(CancellationToken cancellationToken)
        {
            var query = new GetBookingsByUserIdQuery();

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{bookingId}/complete")]
        public async Task<IActionResult> CompleteBooking(Guid bookingId, CancellationToken cancellationToken)
        {
            var command = new CompleteBookingCommand(bookingId);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? NoContent() : HandleFailure(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{bookingId}/confirm")]
        public async Task<IActionResult> ConfirmBooking(Guid bookingId, CancellationToken cancellationToken)
        {
            var command = new ConfirmBookingCommand(bookingId);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? NoContent() : HandleFailure(result);
        }

        [Authorize]
        [HttpPut("{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId, CancellationToken cancellationToken)
        {

            var command = new CancelBookingCommand(bookingId);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? NoContent() : HandleFailure(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{bookingId}/reject")]
        public async Task<IActionResult> RejectBooking(Guid bookingId, CancellationToken cancellationToken)
        {

            var command = new RejectBookingCommand(bookingId);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess ? NoContent() : HandleFailure(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReserveBooking(
            [FromBody] ReserveBookingRequest request,
            CancellationToken cancellationToken)
        {
            var duration = DateRange.Create(request.StartDate, request.EndDate);
            if (duration.IsFailure)
            {
                return HandleFailure(duration);
            }

            var command = new ReserveBookingCommand(request.ApartmentId, duration.Value);

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
                : HandleFailure(result);
        }
    }
}
