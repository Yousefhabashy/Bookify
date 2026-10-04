using Bookify.Domain.Abstractions;

namespace Bookify.Domain.Users
{
    public static class UserErrors
    {
        public static Error NotFound = new(
                "User.NotFound",
                "The user with the specified identifier was not found",
                ErrorType.NotFound
            );

        public static Error InvalidCredentials = new(
                "User.InvalidCredentials",
                "The provided credentials were invalid",
                ErrorType.Failure
            );

        public static Error RegistrationFailed = new(
                "Auth.RegistrationFailed",
                "User registration failed. Email might already be in use.",
                ErrorType.Failure
            );

        public static readonly Error Forbidden = new(
            "User.Forbidden",
            "You do not have permission to access this resource",
            ErrorType.Forbidden
        );

        public static readonly Error Unauthorized = new(
            "User.Unauthorized",
            "You do not have the required permissions to perform this action.",
            ErrorType.Forbidden
        );
    }
}
