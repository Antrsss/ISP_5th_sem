using System.Runtime.CompilerServices;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;
using WEB_353502_ZGIRSKAYA.UI.Services.Authentication;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Extensions
{
    public static class HostingExtensions
    {
        public static void RegisterCustomServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICategoryService, 
                MemoryCategoryService>();
            builder.Services.AddScoped<ICocktailService, MemoryCocktailService>();
            builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));
            builder.Services.AddHttpClient<ITokenAccessor, KeycloakTokenAccessor>();
        }
    }
}
