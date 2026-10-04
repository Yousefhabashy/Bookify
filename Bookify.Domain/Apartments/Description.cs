using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Apartments;

public record Description
{
    public string Value { get; }

    private Description(string value) => Value = value;

    public static Result<Description> Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Description>(new Error("Description.Empty", "Apartment description is required."));
        }

        if (description.Length > 2000)
        {
            return Result.Failure<Description>(new Error("Description.TooLong", "Apartment description cannot exceed 2000 characters."));
        }

        return new Description(description);
    }
}