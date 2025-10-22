using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.API.EndPoints;
using WEB_353502_ZGIRSKAYA.API.Models; // Добавленная строка

namespace WEB_353502_ZGIRSKAYA.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Добавьте эту секцию ДО builder.Build()
            var authServer = builder.Configuration
                .GetSection("AuthServer")
                .Get<AuthServerData>();

            // Добавить сервис аутентификации
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    // Адрес метаданных конфигурации OpenID
                    o.MetadataAddress = $"{authServer.Host}/realms/{authServer.Realm}/.well-known/openid-configuration";
                    // Authority сервера аутентификации
                    o.Authority = $"{authServer.Host}/realms/{authServer.Realm}";
                    // Audience для токена JWT
                    o.Audience = "account";
                    // Запретить HTTPS для использования локальной версии Keycloak
                    o.RequireHttpsMetadata = false;
                });

            // Добавить сервис авторизации с политикой "admin"
            builder.Services.AddAuthorization(opt =>
            {
                opt.AddPolicy("admin", p => p.RequireClaim("realm_access.roles", "POWER-USER"));
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=cocktails.db");
            });

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                await DbInitializer.SeedData(app);
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Добавьте эти middleware в правильном порядке
            app.UseAuthentication(); // ДОЛЖНО БЫТЬ ДО UseAuthorization
            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();
            app.MapCocktailEndpoints();
            app.MapCocktailCategoryEndpoints();

            app.Run();
        }
    }
}