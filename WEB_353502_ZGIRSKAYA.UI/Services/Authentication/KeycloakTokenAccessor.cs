using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;
using WEB_353502_ZGIRSKAYA.UI.Services.Authentication;

namespace WEB_353502_ZGIRSKAYA.UI.Services.Authentication
{
    public class KeycloakTokenAccessor : ITokenAccessor
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly KeycloakData _keycloakData;
        private readonly HttpClient _httpClient;

        public KeycloakTokenAccessor(
            IHttpContextAccessor contextAccessor,
            IOptions<KeycloakData> options,
            HttpClient httpClient)
        {
            _contextAccessor = contextAccessor;
            _keycloakData = options.Value;
            _httpClient = httpClient;
        }

        public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient)
        {
            string token = isClient
                ? await GetClientToken()
                : await GetUserToken();

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
        }

        private async Task<string> GetUserToken()
        {
            var context = _contextAccessor.HttpContext;
            var authSession = await context.AuthenticateAsync("keycloak");

            if (authSession?.Principal == null)
            {
                throw new AuthenticationFailureException("Пользователь неавторизован");
            }

            return await context.GetTokenAsync("keycloak", "access_token");
        }

        private async Task<string> GetClientToken()
        {
            var requestUri = $"{_keycloakData.Host}/realms/{_keycloakData.Realm}/protocol/openid-connect/token";

            HttpContent content = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("client_id", _keycloakData.ClientId),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_secret", _keycloakData.ClientSecret)
            ]);

            var response = await _httpClient.PostAsync(requestUri, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonObject.Parse(jsonString)?["access_token"]?.GetValue<string>() ??
                   throw new InvalidOperationException("Access token not found in response");
        }
    }
}