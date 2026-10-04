using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Users.Register
{
    public sealed record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName
        ) : ICommand<Guid>;
}
