using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookify.Infrastructure.Authentication
{
    public sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
    {
        private readonly ILogger<KeycloakRolesClaimsTransformation> _logger;
        public KeycloakRolesClaimsTransformation(ILogger<KeycloakRolesClaimsTransformation> logger)
        {
            _logger = logger;
        }
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            {
                return Task.FromResult(principal);
            }

            var realmAccessClaim = principal.FindFirst("realm_access");
            if (realmAccessClaim is null)
            {
                return Task.FromResult(principal);
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value, options);

                if (realmAccess?.Roles is null || realmAccess.Roles.Length == 0)
                {
                    return Task.FromResult(principal);
                }

                foreach (var role in realmAccess.Roles)
                {
                    if (!identity.HasClaim(ClaimTypes.Role, role))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        identity.AddClaim(new Claim(identity.RoleClaimType, role));
                    }
                }
            }
            catch (Exception)
            {
                _logger.LogError("Claims Transformation Exception: JSON Parsing failed.");
            }

            return Task.FromResult(principal);
        }

        private sealed record RealmAccess(
            [property: JsonPropertyName("roles")] string[] Roles
        );
    }
}
