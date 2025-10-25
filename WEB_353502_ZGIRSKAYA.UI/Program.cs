using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;
using WEB_353502_ZGIRSKAYA.UI.Models;
using WEB_353502_ZGIRSKAYA.UI.Services;
using WEB_353502_ZGIRSKAYA.UI.Services.Authentication;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var uriData = builder.Configuration.GetSection("UriData").Get<UriData>() ?? new UriData
            {
                ApiUri = "https://localhost:7002/api/"
            };
            builder.Services.AddSingleton(uriData);

            builder.Services.AddHttpClient<ICocktailService, ApiCocktailService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}Cocktail/"));

            builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}CocktailCategory/"));

            builder.Services.AddHttpClient<ITokenAccessor, KeycloakTokenAccessor>();

            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<TempDbContext>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Регистрация конфигурации Keycloak
            builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));

            // Получение данных Keycloak для настройки аутентификации
            var keycloakData = builder.Configuration.GetSection("Keycloak").Get<KeycloakData>();

            // Добавление аутентификации Cookie и OpenIdConnect
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "keycloak";
                })
                .AddCookie()
                .AddOpenIdConnect("keycloak", options =>
                {
                    // Используем путь /realms/... чтобы совпадать с iss в токене (см. ваш пример).
                    options.Authority = $"{keycloakData.Host}/realms/{keycloakData.Realm}";
                    options.ClientId = keycloakData.ClientId;
                    options.ClientSecret = keycloakData.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.Scope.Add("openid");
                    options.SaveTokens = true;
                    options.RequireHttpsMetadata = false;
                    options.MetadataAddress = $"{keycloakData.Host}/realms/{keycloakData.Realm}/.well-known/openid-configuration";

                    // Указываем, чтобы роли в claims были представлены как ClaimTypes.Role
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                            if (claimsIdentity != null)
                            {
                                try
                                {
                                    // 1) realm_access (обычная структура Keycloak)
                                    var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                                    if (!string.IsNullOrEmpty(realmAccess))
                                    {
                                        using var doc = System.Text.Json.JsonDocument.Parse(realmAccess);
                                        if (doc.RootElement.TryGetProperty("roles", out var rolesElement) &&
                                            rolesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                        {
                                            foreach (var r in rolesElement.EnumerateArray())
                                            {
                                                var role = r.GetString();
                                                if (!string.IsNullOrEmpty(role))
                                                    claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                                            }
                                        }
                                    }

                                    // 2) resource_access.{client}.roles
                                    var resourceAccess = context.Principal.FindFirst("resource_access")?.Value;
                                    if (!string.IsNullOrEmpty(resourceAccess))
                                    {
                                        using var doc = System.Text.Json.JsonDocument.Parse(resourceAccess);
                                        foreach (var clientProp in doc.RootElement.EnumerateObject())
                                        {
                                            if (clientProp.Value.TryGetProperty("roles", out var clientRoles) &&
                                                clientRoles.ValueKind == System.Text.Json.JsonValueKind.Array)
                                            {
                                                foreach (var r in clientRoles.EnumerateArray())
                                                {
                                                    var role = r.GetString();
                                                    if (!string.IsNullOrEmpty(role))
                                                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                                                }
                                            }
                                        }
                                    }

                                    // 3) топ-левел "role" или "roles" — как в вашем примере
                                    var topRoleClaim = context.Principal.FindFirst("role")?.Value ?? context.Principal.FindFirst("roles")?.Value;
                                    if (!string.IsNullOrEmpty(topRoleClaim))
                                    {
                                        // Если это JSON-массив или строка с сериализованным массивом — попробуем распарсить
                                        try
                                        {
                                            using var doc = System.Text.Json.JsonDocument.Parse(topRoleClaim);
                                            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                            {
                                                foreach (var r in doc.RootElement.EnumerateArray())
                                                {
                                                    var role = r.GetString();
                                                    if (!string.IsNullOrEmpty(role))
                                                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            // Если не JSON — возможно это просто перечисление или одиночная строка
                                            // Попробуем разделить по запятым
                                            var parts = topRoleClaim.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                            foreach (var part in parts)
                                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, part));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Логирование полезно при отладке — можно заменить на реальное логирование
                                    System.Diagnostics.Trace.TraceWarning($"OnTokenValidated parsing roles failed: {ex.Message}");
                                }
                            }

                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });


            // Добавление политики авторизации
            builder.Services.AddAuthorization(opt =>
                opt.AddPolicy("admin", p => p.RequireRole("POWER-USER")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseCors("AllowAll");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Добавление middleware аутентификации и авторизации
            app.UseAuthentication();
            app.UseAuthorization();

            // Ограничение доступа к страницам Razor Pages только для роли "admin"
            app.MapRazorPages()
               .RequireAuthorization("admin");

            app.MapControllerRoute(
                name: "area",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}