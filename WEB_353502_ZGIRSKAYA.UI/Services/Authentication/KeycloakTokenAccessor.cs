using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;

namespace WEB_353502_ZGIRSKAYA.UI.Services.Authentication
{
    internal class KeycloakTokenAccessor(
        IHttpContextAccessor contextAccessor,
        IOptions<KeycloakData> options,
        HttpClient httpClient) : ITokenAccessor
    {
        public async Task SetAuthorizationHeaderAsync(HttpClient httpClient, bool isClient)
        {
            string token = isClient
             ? await GetClientToken()
             : await GetUserToken();
            httpClient
            .DefaultRequestHeaders
            .Authorization = new AuthenticationHeaderValue("bearer", token);
        }
        async Task<string> GetUserToken()
        {
            var context = contextAccessor.HttpContext;
            var authSession = await context.AuthenticateAsync("keycloak");
            if (authSession?.Principal == null)
            {
                throw new AuthenticationFailureException("Пользователь не авторизован");
            }
            return await context.GetTokenAsync("keycloak", "access_token");
        }

        async Task<string> GetClientToken()
        {
            // Keycloak token endpoint
            var requestUri = 
                $"{options.Value.Host}/realms/{options.Value.Realm}/protocol/openidconnect/token";
            // Http request content
            HttpContent content = new FormUrlEncodedContent([
                new KeyValuePair<string,string>
                ("client_id",options.Value.ClientId),
                 new KeyValuePair<string,string>
                ("grant_type","client_credentials"),
                 new KeyValuePair<string,string>
                ("client_secret",options.Value.ClientSecret)
            ]);
            // send request
            var response = await httpClient.PostAsync(requestUri, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.StatusCode.ToString());
            }
            // extract access token from response
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonObject.Parse(jsonString)["access_token"].GetValue<string>();
        }
    }
}
