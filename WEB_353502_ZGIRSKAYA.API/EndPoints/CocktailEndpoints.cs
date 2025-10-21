using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.API.UseCases;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;

namespace WEB_353502_ZGIRSKAYA.API.EndPoints;

public static class CocktailEndpoints
{
    public static void MapCocktailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Cocktail")
            .WithTags(nameof(Cocktail))
            .DisableAntiforgery();

        group.MapGet("/{category:alpha?}",
            async (IMediator mediator, string? category, int pageNo = 1, int pageSize = 3) =>
            {
                var response = await mediator.Send(new GetListOfCocktails(category, pageNo, pageSize));
                return response.Successfull ? Results.Ok(response) : Results.BadRequest(response);
            })
            .WithName("GetAllCocktails")
            .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<ResponseData<Cocktail>>, NotFound>> (int id, AppDbContext db) =>
        {
            var cocktail = await db.Cocktails.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id);

            return cocktail is not null
                ? TypedResults.Ok(ResponseData<Cocktail>.Success(cocktail))
                : TypedResults.NotFound();
        })
        .WithName("GetCocktailById")
        .WithOpenApi();

        group.MapPost("/", async (
            [FromForm] string cocktail,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("=== BACKEND: START CREATE COCKTAIL ===");
                logger.LogInformation($"Received cocktail JSON: {cocktail}");

                // Настройки для десериализации
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                // Десериализуем JSON
                var newCocktail = JsonSerializer.Deserialize<Cocktail>(cocktail, options);

                if (newCocktail == null)
                {
                    logger.LogError("Failed to deserialize cocktail");
                    return Results.BadRequest(ResponseData<Cocktail>.Error("Invalid cocktail data"));
                }

                logger.LogInformation($"Deserialized cocktail - Name: '{newCocktail.Name}', Description: '{newCocktail.Description}', Price: {newCocktail.Price}");

                // Проверяем, что данные не пустые
                if (string.IsNullOrEmpty(newCocktail.Name) || string.IsNullOrEmpty(newCocktail.Description) || newCocktail.Price <= 0)
                {
                    logger.LogError($"Invalid data after deserialization - Name: '{newCocktail.Name}', Description: '{newCocktail.Description}', Price: {newCocktail.Price}");
                    return Results.BadRequest(ResponseData<Cocktail>.Error("Invalid data after deserialization"));
                }

                // Создаем НОВЫЙ объект в контексте EF
                var cocktailToAdd = new Cocktail
                {
                    Name = newCocktail.Name,
                    Description = newCocktail.Description,
                    Price = newCocktail.Price,
                    Category = newCocktail.Category
                };

                logger.LogInformation($"Cocktail to add - Name: '{cocktailToAdd.Name}', Description: '{cocktailToAdd.Description}', Price: {cocktailToAdd.Price}");

                // Обрабатываем изображение
                if (file != null)
                {
                    logger.LogInformation($"Processing image file: {file.FileName}");
                    var imageUrl = await mediator.Send(new SaveImage(file));
                    cocktailToAdd.PathToPicture = imageUrl;
                    cocktailToAdd.MimeType = file.ContentType;
                }
                else
                {
                    cocktailToAdd.PathToPicture = "images/noimage.jpg";
                    cocktailToAdd.MimeType = "image/jpeg";
                }

                logger.LogInformation($"Before save - Name: '{cocktailToAdd.Name}', Description: '{cocktailToAdd.Description}', Price: {cocktailToAdd.Price}");

                // Добавляем и сохраняем
                db.Cocktails.Add(cocktailToAdd);
                await db.SaveChangesAsync();

                logger.LogInformation($"After save - Name: '{cocktailToAdd.Name}', Description: '{cocktailToAdd.Description}', Price: {cocktailToAdd.Price}");

                // Возвращаем созданный объект
                var response = ResponseData<Cocktail>.Success(cocktailToAdd);
                logger.LogInformation($"Returning response - Name: '{response.Data?.Name}', Description: '{response.Data?.Description}', Price: {response.Data?.Price}");

                return Results.Created($"/api/Cocktail/{cocktailToAdd.Id}", response);
            }
            catch (JsonException ex)
            {
                logger.LogError($"JSON deserialization error: {ex.Message}");
                logger.LogError($"Stack trace: {ex.StackTrace}");
                return Results.BadRequest(ResponseData<Cocktail>.Error($"JSON deserialization error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                logger.LogError($"Error creating cocktail: {ex.Message}");
                logger.LogError($"Stack trace: {ex.StackTrace}");
                return Results.BadRequest(ResponseData<Cocktail>.Error($"Error creating cocktail: {ex.Message}"));
            }
        })
        .WithName("CreateCocktail")
        .WithOpenApi();

        // MapPut ДЛЯ ОБНОВЛЕНИЯ КОКТЕЙЛЯ
        group.MapPut("/{id}", async Task<Results<Ok<ResponseData<Cocktail>>, NotFound, BadRequest<ResponseData<Cocktail>>>> (
            int id,
            [FromForm] string cocktail,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("=== BACKEND: START UPDATE COCKTAIL ===");
                logger.LogInformation($"Updating cocktail ID: {id}");
                logger.LogInformation($"Received cocktail JSON: {cocktail}");

                var existingCocktail = await db.Cocktails
                    .FirstOrDefaultAsync(model => model.Id == id);

                if (existingCocktail == null)
                {
                    logger.LogWarning($"Cocktail with ID {id} not found");
                    return TypedResults.NotFound();
                }

                // Настройки для десериализации (такие же как в Create)
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                // Десериализуем JSON
                var updatedData = JsonSerializer.Deserialize<Cocktail>(cocktail, options);

                if (updatedData == null)
                {
                    logger.LogError("Failed to deserialize cocktail data for update");
                    return TypedResults.BadRequest(ResponseData<Cocktail>.Error("Invalid cocktail data"));
                }

                logger.LogInformation($"Deserialized update data - Name: '{updatedData.Name}', Description: '{updatedData.Description}', Price: {updatedData.Price}");

                // Проверяем, что данные не пустые
                if (string.IsNullOrEmpty(updatedData.Name) || string.IsNullOrEmpty(updatedData.Description) || updatedData.Price <= 0)
                {
                    logger.LogError($"Invalid data after deserialization - Name: '{updatedData.Name}', Description: '{updatedData.Description}', Price: {updatedData.Price}");
                    return TypedResults.BadRequest(ResponseData<Cocktail>.Error("Invalid data after deserialization"));
                }

                // Логируем текущие значения перед обновлением
                logger.LogInformation($"Before update - Name: '{existingCocktail.Name}', Description: '{existingCocktail.Description}', Price: {existingCocktail.Price}");

                // Обновляем только необходимые поля
                existingCocktail.Name = updatedData.Name;
                existingCocktail.Description = updatedData.Description;
                existingCocktail.Price = updatedData.Price;

                // Обновляем изображение если есть файл
                if (file != null)
                {
                    logger.LogInformation($"Processing image file for update: {file.FileName}");
                    var imageUrl = await mediator.Send(new SaveImage(file));
                    existingCocktail.PathToPicture = imageUrl;
                    existingCocktail.MimeType = file.ContentType;
                }
                else
                {
                    logger.LogInformation("No image file provided for update, keeping existing image");
                }

                // Логируем значения после обновления (перед сохранением)
                logger.LogInformation($"After update (before save) - Name: '{existingCocktail.Name}', Description: '{existingCocktail.Description}', Price: {existingCocktail.Price}");

                await db.SaveChangesAsync();

                // Логируем значения после сохранения
                logger.LogInformation($"After save - Name: '{existingCocktail.Name}', Description: '{existingCocktail.Description}', Price: {existingCocktail.Price}");

                return TypedResults.Ok(ResponseData<Cocktail>.Success(existingCocktail));
            }
            catch (JsonException ex)
            {
                logger.LogError($"JSON deserialization error in update: {ex.Message}");
                logger.LogError($"Stack trace: {ex.StackTrace}");
                return TypedResults.BadRequest(ResponseData<Cocktail>.Error($"JSON deserialization error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating cocktail: {ex.Message}");
                logger.LogError($"Stack trace: {ex.StackTrace}");
                return TypedResults.BadRequest(ResponseData<Cocktail>.Error($"Error updating cocktail: {ex.Message}"));
            }
        })
        .WithName("UpdateCocktail")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok<ResponseData<string>>, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Cocktails
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1
                ? TypedResults.Ok(ResponseData<string>.Success("Cocktail deleted successfully"))
                : TypedResults.NotFound();
        })
        .WithName("DeleteCocktail")
        .WithOpenApi();
    }
}