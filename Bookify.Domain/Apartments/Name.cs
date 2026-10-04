using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Apartments;

public record Name
{
    public string Value { get; }

    private Name(string value) => Value = value;

    public static Result<Name> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Name>(new Error("Name.Empty", "Apartment name is required."));
        }

        if (name.Length > 200)
        {
            return Result.Failure<Name>(new Error("Name.TooLong", "Apartment name cannot exceed 200 characters."));
        }

        return new Name(name);
    }
}