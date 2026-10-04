namespace Bookify.Infrastructure.Authentication
{
    public sealed class KeycloakOptions
    {
        public string AdminUrl { get; init; } = string.Empty;
        public string TokenUrl { get; init; } = string.Empty;
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }
}
