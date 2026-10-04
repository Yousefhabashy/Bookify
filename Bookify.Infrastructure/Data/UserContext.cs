using Bookify.Application.Abstractions;
using Bookify.Application.Abstractions.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bookify.Infrastructure.Data
{
    public sealed class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal Principal =>
            _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("User context is unavailable");

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        public Guid UserId => Guid.Parse(
            Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Principal.FindFirstValue("sub")
            ?? throw new UserProfileIncompleteException("User id claim is missing")
            );

        public string Email =>
            Principal.FindFirstValue(ClaimTypes.Email)
            ?? Principal.FindFirstValue("email")
            ?? throw new UserProfileIncompleteException("Email claim is missing");

        public string FirstName =>
            Principal.FindFirstValue("given_name")
            ?? throw new UserProfileIncompleteException("First name claim is missing");

        public string LastName =>
            Principal.FindFirstValue("family_name")
            ?? throw new UserProfileIncompleteException("Last name claim is missing");

        public bool IsAdmin =>
            _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;

    }
}
