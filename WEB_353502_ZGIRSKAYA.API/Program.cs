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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=cocktails.db");
            });

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // CORS - ÏÐÀÂÈËÜÍÎ
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

            // ÏÐÀÂÈËÜÍÛÉ ÏÎÐßÄÎÊ MIDDLEWARE:
            app.UseHttpsRedirection();

            app.UseRouting(); // ? ÄÎËÆÍÎ ÁÛÒÜ ÏÅÐÂÛÌ

            app.UseCors("AllowAll"); // ? ÏÎÑËÅ UseRouting()

            // Íàñòðîéêà ñòàòè÷åñêèõ ôàéëîâ ñ MIME types
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                RequestPath = "",
                ServeUnknownFileTypes = true, // Âàæíî!
                DefaultContentType = "image/jpeg",
                OnPrepareResponse = ctx =>
                {
                    Console.WriteLine($"Îáñëóæèâàþ ôàéë: {ctx.File.PhysicalPath}");
                }
            });

            app.UseAuthorization();

            app.MapControllers();
            app.MapCocktailEndpoints();
            app.MapCocktailCategoryEndpoints();

            app.Run();
        }
    }
}