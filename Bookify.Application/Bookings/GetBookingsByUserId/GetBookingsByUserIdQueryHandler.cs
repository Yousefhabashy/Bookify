using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Bookings.GetBookingByUserId;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Bookings.GetBookingsByUserId
{
    internal sealed class GetBookingsByUserIdQueryHandler : IQueryHandler<GetBookingsByUserIdQuery, IReadOnlyList<BookingResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContext _userContext;
        public GetBookingsByUserIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<Result<IReadOnlyList<BookingResponse>>> Handle(GetBookingsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    ApartmentId = b.ApartmentId,
                    UserId = b.UserId,

                    Status = b.Status.ToString(),

                    PriceAmount = b.PriceForPeriod.Amount,
                    PriceCurrency = b.PriceForPeriod.Currency.Code,

                    CleaningFeeAmount = b.CleaningFee.Amount,
                    CleaningFeeCurrency = b.CleaningFee.Currency.Code,

                    AmenitiesUpChargeAmount = b.AmenitiesUpCharge.Amount,
                    AmenitiesUpChargeCurrency = b.AmenitiesUpCharge.Currency.Code,

                    TotalPriceAmount = b.TotalPrice.Amount,
                    TotalPriceCurrency = b.TotalPrice.Currency.Code,

                    DurationEnd = b.Duration.End,
                    DurationStart = b.Duration.Start,

                    CreatedOnUtc = b.CreatedOnUtc,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // even if empty will return  a success with empty list 
            return bookings;
        }
    }
}
