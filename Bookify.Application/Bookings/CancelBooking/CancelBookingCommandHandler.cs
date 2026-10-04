using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Bookings.CancelBooking
{
    internal sealed class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUserContext _userContext;
        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IUserContext userContext
            )
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _userContext = userContext;
        }
        public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);

            if (booking is null)
            {
                return Result.Failure(BookingErrors.NotFound);
            }

            if (booking.UserId != _userContext.UserId)
            {
                return Result.Failure(BookingErrors.NotFound);
            }

            var result = booking.Cancel(_dateTimeProvider.UtcNow);

            if (result.IsFailure)
            {
                return result;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
