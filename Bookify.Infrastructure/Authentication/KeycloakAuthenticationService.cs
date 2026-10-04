using Bookify.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Bookify.Infrastructure.Authentication
{
    internal sealed class KeycloakAuthenticationService : IAuthenticationService
    {
        private const string PasswordCredentialType = "password";

        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _keycloakOptions;

        public KeycloakAuthenticationService(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
        {
            _httpClient = httpClient;
            _keycloakOptions = keycloakOptions.Value;
        }

        public async Task<string> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            CancellationToken cancellationToken = default
            )
        {
            await EnsureAdminTokenIsAttachedAsync(cancellationToken);

            var userRepresentation = new
            {
                username = email,
                email,
                firstName,
                lastName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new {
                        temporary = false,
                        type = PasswordCredentialType,
                        value = password
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("users", userRepresentation, cancellationToken);

            response.EnsureSuccessStatusCode();

            var locationHeader = response.Headers.Location!.ToString();
            var userId = locationHeader.Split('/').Last();

            return userId;
        }

        private async Task EnsureAdminTokenIsAttachedAsync(CancellationToken cancellationToken)
        {
            var authRequestParameters = new KeyValuePair<string, string>[]
            {
                new("client_id", _keycloakOptions.ClientId),
                new("client_secret", _keycloakOptions.ClientSecret),
                new("grant_type", "client_credentials")
            };

            var authorizationRequestContent = new FormUrlEncodedContent(authRequestParameters);

            var response = await _httpClient.PostAsync(_keycloakOptions.TokenUrl, authorizationRequestContent, cancellationToken);

            response.EnsureSuccessStatusCode();

            var authorizationToken = await response.Content.ReadFromJsonAsync<AuthorizationToken>(cancellationToken: cancellationToken);

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authorizationToken!.AccessToken);
        }

        private sealed class AuthorizationToken
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; init; } = string.Empty;
        }
    }
}
