using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;

namespace Bookify.Application.Bookings.ReserveBooking
{
    internal sealed class ReserveBookingCommandHandler : ICommandHandler<ReserveBookingCommand, Guid>
    {
        private readonly IApartmentRepository _apartmentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PricingService _pricingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUserContext _userContext;

        public ReserveBookingCommandHandler(
            IApartmentRepository apartmentRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            PricingService pricingService,
            IUserContext userContext
            )
        {
            _apartmentRepository = apartmentRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _pricingService = pricingService;
            _userContext = userContext;
        }

        public async Task<Result<Guid>> Handle(ReserveBookingCommand request, CancellationToken cancellationToken)
        {
            // Find Apartment
            var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId, cancellationToken);
            if (apartment is null)
            {
                return Result.Failure<Guid>(ApartmentErrors.NotFound);
            }

            // check Overlaping Booking 
            bool isOverlaping = await _bookingRepository.IsOverlappingAsync(apartment, request.Duration, cancellationToken);
            if (isOverlaping)
            {
                return Result.Failure<Guid>(BookingErrors.Overlap);
            }

            var userId = _userContext.UserId;

            var bookingResult = Booking.Reserve(
                apartment,
                userId,
                request.Duration,
                _dateTimeProvider.UtcNow,
                _pricingService);

            if (bookingResult.IsFailure)
            {
                return Result.Failure<Guid>(bookingResult.Error);
            }

            var booking = bookingResult.Value;

            _bookingRepository.Add(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }
    }
}
