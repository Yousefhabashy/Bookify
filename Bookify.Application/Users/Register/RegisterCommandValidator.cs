using FluentValidation;

namespace Bookify.Application.Users.Register
{
    internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(c => c.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("First name cannot be empty.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("Last name cannot be empty.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
        }
    }
}
