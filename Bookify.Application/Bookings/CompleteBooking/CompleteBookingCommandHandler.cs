using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;

namespace Bookify.Application.Bookings.CompleteBooking
{
    internal sealed class CompleteBookingCommandHandler : ICommandHandler<CompleteBookingCommand>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUserContext _userContext;
        public CompleteBookingCommandHandler(
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

        public async Task<Result> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
        {
            if (!_userContext.IsAdmin) 
            {
                return Result.Failure(UserErrors.Unauthorized);
            }

            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
            {
                return Result.Failure(BookingErrors.NotFound);
            }

            var result = booking.Complete(_dateTimeProvider.UtcNow);

            if (result.IsFailure)
            {
                return result;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
