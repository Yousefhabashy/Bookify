using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Users
{
    public sealed record FirstName
    {
        public string Value { get; }

        private FirstName(string value)
        {
            Value = value;
        }

        public static Result<FirstName> Create(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return Result.Failure<FirstName>(new Error("FirstName.Empty", "First name is required."));
            }

            if (firstName.Length > 100)
            {
                return Result.Failure<FirstName>(new Error("FirstName.TooLong", "First name cannot exceed 100 characters."));
            }

            return new FirstName(firstName);
        }
    }
}
