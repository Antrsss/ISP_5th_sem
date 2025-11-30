using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using WEB_353502_ZGIRSKAYA.BlazorWasm.Services;

namespace WEB_353502_ZGIRSKAYA.BlazorWasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // HttpClient дл€ API
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7002/")
            });

            // –егистраци€ сервиса данных с зависимостью от IAccessTokenProvider
            builder.Services.AddScoped<IDataService, DataService>();

            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.Authority = "http://localhost:8080/realms/zgirskaya";
                options.ProviderOptions.ClientId = "WebAssemblyClient";
                options.ProviderOptions.RedirectUri = "https://localhost:7257/authentication/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = "https://localhost:7257/authentication/logout-callback";
                options.ProviderOptions.ResponseType = "code";

                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("email");

                options.UserOptions.NameClaim = "preferred_username";
            });

            await builder.Build().RunAsync();
        }
    }
}