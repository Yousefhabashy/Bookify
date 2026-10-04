using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Users
{
    public sealed record LastName
    {
        public string Value { get; }
        private LastName(string value)
        {
            Value = value;
        }

        public static Result<LastName> Create(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
            {
                return Result.Failure<LastName>(new Error("LastName.Empty", "Last name is required."));
            }

            if (lastName.Length > 100)
            {
                return Result.Failure<LastName>(new Error("LastName.TooLong", "Last name cannot exceed 100 characters."));
            }

            return new LastName(lastName);
        }
    }

}
