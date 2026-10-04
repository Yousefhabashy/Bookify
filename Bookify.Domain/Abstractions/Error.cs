namespace Bookify.Domain.Abstractions
{

    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        Forbidden = 4
    }

    public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
    {
        public static Error None = new(string.Empty, string.Empty);
        public static Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Failure);
    }
}
