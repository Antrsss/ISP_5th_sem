using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.API.EndPoints;

namespace WEB_353502_ZGIRSKAYA.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=cocktails.db");
            });

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // ДОБАВЬТЕ CORS
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
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                await DbInitializer.SeedData(app);
            }

            // ПРАВИЛЬНЫЙ ПОРЯДОК MIDDLEWARE:
            app.UseHttpsRedirection();

            // ДОБАВЬТЕ CORS перед другими middleware
            app.UseCors("AllowAll");

            // Настройка статических файлов
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                RequestPath = "",
                // ДОБАВЬТЕ ContentTypeProvider для правильных MIME types
                ContentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider
                {
                    Mappings =
                    {
                        [".jpg"] = "image/jpeg",
                        [".jpeg"] = "image/jpeg",
                        [".png"] = "image/png",
                        [".gif"] = "image/gif",
                        [".webp"] = "image/webp"
                    }
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();
            app.MapCocktailEndpoints();
            app.MapCocktailCategoryEndpoints();

            app.Run();
        }
    }
}