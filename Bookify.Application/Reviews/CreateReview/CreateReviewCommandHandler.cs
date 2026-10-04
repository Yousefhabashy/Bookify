using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Bookings;
using Bookify.Domain.Reviews;

namespace Bookify.Application.Reviews.CreateReview
{
    internal sealed class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, Guid>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateReviewCommandHandler(
            IBookingRepository bookingRepository,
            IReviewRepository reviewRepository,
            IUserContext userContext,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider
            )
        {
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return Result.Failure<Guid>(BookingErrors.NotFound);

            var userId = _userContext.UserId;
            if (booking.UserId != userId)
                return Result.Failure<Guid>(BookingErrors.NotFound);

            if (await _reviewRepository.ExistsByBookingIdAsync(booking.Id, cancellationToken))
                return Result.Failure<Guid>(ReviewErrors.AlreadyExists);

            var rating = Rating.Create(request.Rating);
            if (rating.IsFailure)
                return Result.Failure<Guid>(rating.Error);

            var comment = Comment.Create(request.Comment);
            if (comment.IsFailure)
                return Result.Failure<Guid>(comment.Error);

            var review = Review.Create(
                booking,
                rating.Value,
                comment.Value,
                _dateTimeProvider.UtcNow
                );

            if (review.IsFailure)
                return Result.Failure<Guid>(review.Error);

            _reviewRepository.Add(review.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return review.Value.Id;
        }
    }
}
