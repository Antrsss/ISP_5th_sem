using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.API.UseCases;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using Xunit;

namespace WEB_353502_ZGIRSKAYA.Tests.Services
{
    public class CocktailServiceTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _contextOptions;

        public CocktailServiceTests()
        {
            // Создаем и открываем соединение SQLite in-memory
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            // Настраиваем параметры DbContext
            _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Создаем и инициализируем базу данных
            using var context = new AppDbContext(_contextOptions);
            context.Database.EnsureCreated();
        }

        private AppDbContext CreateContext() => new AppDbContext(_contextOptions);

        public void Dispose()
        {
            _connection?.Dispose();
        }

        [Fact]
        public async Task GetListOfCocktails_ReturnsFirstPageOfThreeItems_ByDefault()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 1, 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено: Successfull (свойство)
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.CurrentPage);
            Assert.Equal(3, result.Data.Items.Count);
            Assert.Equal(2, result.Data.TotalPages);
            Assert.Equal(3, result.Data.PageSize);
        }

        [Fact]
        public async Task GetListOfCocktails_ReturnsCorrectPage_WhenPageNumberSpecified()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 2, 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.CurrentPage);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(2, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetListOfCocktails_FiltersByCategory_Correctly()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails("alcoholic", 1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.All(result.Data.Items, cocktail =>
                Assert.Equal("alcoholic", cocktail.Category?.NormilisedName));
            Assert.Equal(3, result.Data.Items.Count);
        }

        [Fact]
        public async Task GetListOfCocktails_ReturnsEmptyList_WhenCategoryNotFound()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails("nonexistent", 1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(0, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetListOfCocktails_LimitsPageSize_ToMaxPageSize()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 1, 50);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Equal(20, result.Data.PageSize);
        }

        [Fact]
        public async Task GetListOfCocktails_UsesDefaultPageSize_WhenInvalid()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 1, 0);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.PageSize);
        }

        [Fact]
        public async Task GetListOfCocktails_ReturnsEmptyList_WhenPageExceedsTotalPages()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 10, 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(10, result.Data.CurrentPage);
        }

        [Fact]
        public async Task GetListOfCocktails_OrdersByName_ByDefault()
        {
            // Arrange
            using var context = CreateContext();
            await SeedTestData(context);

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 1, 10);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);

            var names = result.Data.Items.Select(c => c.Name).ToList();
            var sortedNames = names.OrderBy(n => n).ToList();
            Assert.Equal(sortedNames, names);
        }

        [Fact]
        public async Task GetListOfCocktails_HandlesEmptyDatabase_Gracefully()
        {
            // Arrange
            using var context = CreateContext();

            var handler = new GetListOfCocktailsHandler(context);
            var query = new GetListOfCocktails(null, 1, 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Successfull); // Исправлено
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.Items);
            Assert.Equal(1, result.Data.CurrentPage);
            Assert.Equal(0, result.Data.TotalPages);
        }

        [Fact]
        public async Task GetListOfCocktails_ReturnsSuccessFalse_OnException()
        {
            // Arrange
            // Создаем контекст с невалидной строкой подключения
            var invalidOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Invalid Connection String")
                .Options;

            using var invalidContext = new AppDbContext(invalidOptions);
            var handler = new GetListOfCocktailsHandler(invalidContext);
            var query = new GetListOfCocktails(null, 1, 3);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Successfull);
            Assert.NotNull(result.ErrorMessage);
        }

        private async Task SeedTestData(AppDbContext context)
        {
            // Создаем категории
            var alcoholicCategory = new CocktailCategory
            {
                Id = 1,
                Name = "Алкогольные",
                NormilisedName = "alcoholic"
            };

            var nonAlcoholicCategory = new CocktailCategory
            {
                Id = 2,
                Name = "Безалкогольные",
                NormilisedName = "non-alcoholic"
            };

            context.CocktailCategories.AddRange(alcoholicCategory, nonAlcoholicCategory);

            // Создаем коктейли
            var cocktails = new List<Cocktail>
            {
                new Cocktail { Id = 1, Name = "Апероль Шприц", Description = "Итальянский аперитив", Price = 450, Category = alcoholicCategory },
                new Cocktail { Id = 2, Name = "Беллини", Description = "Фруктовый коктейль", Price = 500, Category = alcoholicCategory },
                new Cocktail { Id = 3, Name = "Космополитен", Description = "Классический коктейль", Price = 550, Category = alcoholicCategory },
                new Cocktail { Id = 4, Name = "Мохито", Description = "Освежающий мятный коктейль", Price = 350, Category = nonAlcoholicCategory },
                new Cocktail { Id = 5, Name = "Лимонад", Description = "Домашний лимонад", Price = 200, Category = nonAlcoholicCategory }
            };

            context.Cocktails.AddRange(cocktails);
            await context.SaveChangesAsync();
        }
    }
}