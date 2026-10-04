using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Users
{
    public record Email
    {
        public string Value { get; }

        private Email(string value) => Value = value;

        public static Result<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result.Failure<Email>(new Error("Email.Empty", "Email is required."));
            }

            if (!email.Contains('@') || !email.Split('@')[1].Contains('.'))
            {
                return Result.Failure<Email>(new Error("Email.Invalid", "Email format is invalid."));
            }

            return new Email(email);
        }
    }
}
