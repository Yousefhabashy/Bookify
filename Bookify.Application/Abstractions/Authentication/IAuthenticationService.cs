namespace Bookify.Application.Abstractions.Authentication
{
    public interface IAuthenticationService
    {
        Task<string> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default
            );
    }
}
