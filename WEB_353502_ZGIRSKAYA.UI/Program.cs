using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.UI.Models;
using WEB_353502_ZGIRSKAYA.UI.Services;
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

            // Получение UriData ПЕРЕД регистрацией сервисов
            var uriData = builder.Configuration.GetSection("UriData").Get<UriData>() ?? new UriData
            {
                ApiUri = "https://localhost:7002/api/"
            };
            builder.Services.AddSingleton(uriData);

            builder.Services.AddHttpClient<ICocktailService, ApiCocktailService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}Cocktail/")); // Cocktail вместо Cocktails

            builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(opt =>
                opt.BaseAddress = new Uri($"{uriData.ApiUri}Category/")); // Category вместо Categories

            // CORS должен быть ДО других сервисов
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
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
                // В разработке тоже используем CORS
                app.UseCors("AllowAll");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}