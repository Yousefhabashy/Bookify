using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Reviews
{
    public static class ReviewErrors
    {
        public static readonly Error NotEligible = new(
            "Review.NotEligible",
            "The review is not eligible because the booking is not yet completed",
            ErrorType.Failure
            );

        public static readonly Error AlreadyExists = new(
            "Review.AlreadyExists",
            "A review for this booking already exists",
            ErrorType.Conflict
            );

        public static readonly Error NotFound = new(
            "Review.NotFound",
            "The review was not found",
            ErrorType.NotFound
            );
    }
}
