using FluentValidation;

namespace Bookify.Application.Reviews.CreateReview
{
    internal sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(c => c.BookingId)
                .NotEmpty();

            RuleFor(c => c.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(c => c.Comment)
                .NotEmpty()
                .MaximumLength(400);
        }
    }
}
