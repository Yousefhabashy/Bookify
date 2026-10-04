using Bookify.Application.Abstractions.Authentication;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Bookify.Application.Users.Register
{
    internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<RegisterCommandHandler> _logger;
        public RegisterCommandHandler(IAuthenticationService authService, ILogger<RegisterCommandHandler> logger)
        {
            _authService = authService;
            _logger = logger;

        }

        public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            string userIdString;

            try
            {
                userIdString = await _authService.RegisterAsync(
                    request.Email, request.Password, request.FirstName, request.LastName, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Registration failed for email {Email}", request.Email);
                return Result.Failure<Guid>(UserErrors.RegistrationFailed);
            }

            if (!Guid.TryParse(userIdString, out var userId))
                return Result.Failure<Guid>(UserErrors.InvalidCredentials);

            return userId;
        }
    }
}
