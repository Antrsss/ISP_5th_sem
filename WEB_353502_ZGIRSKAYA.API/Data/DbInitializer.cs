using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WEB_353502_ZGIRSKAYA.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            // Получение контекста БД из сервисов
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var baseUrl = app.Configuration["AppSettings:BaseUrl"];

            // Выполнение миграций
            await context.Database.MigrateAsync();

            // Проверяем, есть ли уже данные
            if (await context.CocktailCategories.AnyAsync())
            {
                return; // База уже заполнена
            }

            // Добавляем категории коктейлей
            var categories = new List<CocktailCategory>
            {
                new CocktailCategory { Name = "Классические коктейли", NormilisedName = "classic" },
                new CocktailCategory { Name = "Кремовые коктейли", NormilisedName = "cream" },
                new CocktailCategory { Name = "Мартини", NormilisedName = "martini" }
            };

            await context.CocktailCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // Добавляем коктейли с полными URL изображений
            var cocktails = new List<Cocktail>
            {
                new Cocktail
                {
                    Name = "Мохито",
                    Description = "Освежающий коктейль с мятой и лаймом",
                    Price = 350.0,
                    Category = categories[0],
                    PathToPicture = $"{baseUrl}/Images/mojito.jpg",
                    MimeType = "image/jpeg"
                },
                new Cocktail
                {
                    Name = "Пина Колада",
                    Description = "Тропический коктейль с кокосом и ананасом",
                    Price = 420.0,
                    Category = categories[1],
                    PathToPicture = $"{baseUrl}/Images/colada.jpg",
                    MimeType = "image/jpeg"
                },
                new Cocktail
                {
                    Name = "Грассхоппер",
                    Description = "Кремовый мятный коктейль",
                    Price = 380.0,
                    Category = categories[1],
                    PathToPicture = $"{baseUrl}/Images/grasshopper.jpg",
                    MimeType = "image/jpeg"
                },
                new Cocktail
                {
                    Name = "Мартини",
                    Description = "Классический алкогольный коктейль",
                    Price = 450.0,
                    Category = categories[2],
                    PathToPicture = $"{baseUrl}/Images/martini.jpg",
                    MimeType = "image/jpeg"
                }
            };

            await context.Cocktails.AddRangeAsync(cocktails);
            await context.SaveChangesAsync();
        }
    }
}