using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WEB_353502_ZGIRSKAYA.UI.HelperClasses;
using WEB_353502_ZGIRSKAYA.UI.Models;
using WEB_353502_ZGIRSKAYA.UI.Services;
using WEB_353502_ZGIRSKAYA.UI.Services.Authentication;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpContextAccessor();

            var uriData = builder.Configuration.GetSection("UriData").Get<UriData>() ?? new UriData
            {
                ApiUri = "https://localhost:7002/api/"
            };
            builder.Services.AddSingleton(uriData);

            builder.Services.AddHttpClient<ICocktailService, ApiCocktailService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}Cocktail/"));

            builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}CocktailCategory/"));

            builder.Services.AddScoped<ITokenAccessor, KeycloakTokenAccessor>();
            builder.Services.AddHttpClient();

            builder.Services.AddRazorPages();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));

            var keycloakData = builder.Configuration.GetSection("Keycloak").Get<KeycloakData>();

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "keycloak";
                })
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                })
                .AddOpenIdConnect("keycloak", options =>
                {
                    options.Authority = $"{keycloakData.Host}/realms/{keycloakData.Realm}";
                    options.ClientId = keycloakData.ClientId;
                    options.ClientSecret = keycloakData.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");

                    options.SaveTokens = true;
                    options.RequireHttpsMetadata = false;

                    options.MetadataAddress = $"{keycloakData.Host}/realms/{keycloakData.Realm}/.well-known/openid-configuration";

                    options.UseTokenLifetime = false;
                    options.RefreshOnIssuerKeyNotFound = true;

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                        NameClaimType = "name",
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
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
                                    var topRoleClaim = context.Principal.FindFirst("role")?.Value ?? context.Principal.FindFirst("roles")?.Value;
                                    if (!string.IsNullOrEmpty(topRoleClaim))
                                    {
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
                                            var parts = topRoleClaim.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                            foreach (var part in parts)
                                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, part));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Trace.TraceWarning($"OnTokenValidated parsing roles failed: {ex.Message}");
                                }
                            }

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = context =>
                        {
                            context.Response.Redirect("/Home/Error");
                            context.HandleResponse();
                            return System.Threading.Tasks.Task.CompletedTask;
                        },

                        OnAccessDenied = context =>
                        {
                            context.Response.Redirect("/Home/AccessDenied");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(opt =>
                opt.AddPolicy("admin", p => p.RequireRole("POWER-USER")));

            builder.Services.AddScoped<IFileService, LocalFileService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(1);
            });

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
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages()
               .RequireAuthorization("admin");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}