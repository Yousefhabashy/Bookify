using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Bookings.AutoRejectBooking
{
    internal sealed class AutoRejectBookingHandler : ICommandHandler<AutoRejectBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AutoRejectBookingHandler(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider
            )
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(AutoRejectBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
            {
                return Result.Failure(BookingErrors.NotFound);
            }

            var result = booking.Reject(_dateTimeProvider.UtcNow);

            if (result.IsFailure)
            {
                return result;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
