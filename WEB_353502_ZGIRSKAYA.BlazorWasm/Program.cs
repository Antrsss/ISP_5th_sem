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
                builder.Configuration.Bind("Keycloak", options.ProviderOptions);
                options.UserOptions.NameClaim = "preferred_username";
            });

            await builder.Build().RunAsync();
        }
    }
}