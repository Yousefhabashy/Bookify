using Bookify.Application.Apartments.CreateApartment;
using Bookify.Application.Apartments.GetApartmentById;
using Bookify.Application.Apartments.SearchApartments;
using Bookify.Domain.Bookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments
{
    [Route("api/apartments")]
    public class ApartmentsController : ApiController
    {
        public ApartmentsController(ISender sender) : base(sender)
        {
        }

        // apartments/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetApartmentByIdQuery(id);

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken
            )
        {
            var dateRange = DateRange.Create(startDate, endDate);

            if (dateRange.IsFailure)
            {
                return HandleFailure(dateRange);
            }

            var query = new SearchApartmentsQuery(dateRange.Value);

            var result = await Sender.Send(query, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateApartment(
            [FromBody] CreateApartmentRequest request,
            CancellationToken cancellationToken
            )
        {
            var command = new CreateApartmentCommand(
                request.Name,
                request.Description,
                request.Country,
                request.State,
                request.City,
                request.Street,
                request.ZipCode,
                request.PriceAmount,
                request.PriceCurrency,
                request.CleaningFeeAmount,
                request.CleaningFeeCurrency,
                request.Amenities
                );

            var result = await Sender.Send(command, cancellationToken);

            return result.IsSuccess
                    ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
                    : HandleFailure(result);
        }
    }
}
