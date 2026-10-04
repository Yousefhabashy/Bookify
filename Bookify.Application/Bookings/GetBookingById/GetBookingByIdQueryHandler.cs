using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Application.Data;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Microsoft.EntityFrameworkCore;

namespace Bookify.Application.Bookings.GetBookingById
{
    internal sealed class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingResponse>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserContext _userContext;
        public GetBookingByIdQueryHandler(IApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<Result<BookingResponse>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            var isAdmin = _userContext.IsAdmin;


            var booking = await _context.Bookings
                .Where(b => b.Id == request.Id && (b.UserId == userId || isAdmin))
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
                .FirstOrDefaultAsync(cancellationToken);

            if (booking == null)
            {
                return Result.Failure<BookingResponse>(BookingErrors.NotFound);
            }
            return booking;
        }
    }
}
