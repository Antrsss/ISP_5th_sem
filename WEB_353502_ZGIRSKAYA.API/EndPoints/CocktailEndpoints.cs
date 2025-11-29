using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
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

        // GET для всех авторизованных пользователей
        group.MapGet("/{category:alpha?}",
            async (IMediator mediator, HybridCache cache, string? category, int pageNo = 1, int pageSize = 3) =>
            {
                var data = await cache.GetOrCreateAsync(
                    $"cocktails_{category ?? "all"}_{pageNo}_{pageSize}",
                    async token => await mediator.Send(new GetListOfCocktails(category, pageNo, pageSize)),
                    options: new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(1),
                        LocalCacheExpiration = TimeSpan.FromSeconds(30)
                    }
                );
                return data.Successfull ? Results.Ok(data) : Results.BadRequest(data);
            })
            .WithName("GetAllCocktails")
            .WithOpenApi()
            .RequireAuthorization(); // Любой авторизованный

        group.MapGet("/{id}",
            async Task<Results<Ok<ResponseData<Cocktail>>, NotFound>> (int id, AppDbContext db) =>
            {
                var cocktail = await db.Cocktails.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.Id == id);

                return cocktail is not null
                    ? TypedResults.Ok(ResponseData<Cocktail>.Success(cocktail))
                    : TypedResults.NotFound();
            })
            .WithName("GetCocktailById")
            .WithOpenApi()
            .RequireAuthorization(); // Любой авторизованный

        // POST, PUT, DELETE только для админов
        group.MapPost("/",
            async ([FromForm] string cocktail, [FromForm] IFormFile? file, AppDbContext db, IMediator mediator) =>
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    var newCocktail = JsonSerializer.Deserialize<Cocktail>(cocktail, options);

                    if (newCocktail == null)
                        return Results.BadRequest(ResponseData<Cocktail>.Error("Invalid cocktail data"));

                    if (string.IsNullOrEmpty(newCocktail.Name) || string.IsNullOrEmpty(newCocktail.Description) || newCocktail.Price <= 0)
                        return Results.BadRequest(ResponseData<Cocktail>.Error("Invalid data after deserialization"));

                    var cocktailToAdd = new Cocktail
                    {
                        Name = newCocktail.Name,
                        Description = newCocktail.Description,
                        Price = newCocktail.Price
                    };

                    if (newCocktail.Category != null && newCocktail.Category.Id > 0)
                    {
                        var categoryFromDb = await db.CocktailCategories
                            .FirstOrDefaultAsync(c => c.Id == newCocktail.Category.Id);

                        if (categoryFromDb != null)
                            cocktailToAdd.Category = categoryFromDb;
                    }

                    if (file != null)
                    {
                        var imageUrl = await mediator.Send(new SaveImage(file));
                        cocktailToAdd.PathToPicture = imageUrl;
                        cocktailToAdd.MimeType = file.ContentType;
                    }
                    else
                    {
                        cocktailToAdd.PathToPicture = "images/noimage.jpg";
                        cocktailToAdd.MimeType = "image/jpeg";
                    }

                    db.Cocktails.Add(cocktailToAdd);
                    await db.SaveChangesAsync();

                    var response = ResponseData<Cocktail>.Success(cocktailToAdd);

                    return Results.Created($"/api/Cocktail/{cocktailToAdd.Id}", response);
                }
                catch (JsonException ex)
                {
                    return Results.BadRequest(ResponseData<Cocktail>.Error($"JSON deserialization error: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ResponseData<Cocktail>.Error($"Error creating cocktail: {ex.Message}"));
                }
            })
            .WithName("CreateCocktail")
            .WithOpenApi()
            .RequireAuthorization("admin");

        group.MapPut("/{id}",
            async Task<Results<Ok<ResponseData<Cocktail>>, NotFound, BadRequest<ResponseData<Cocktail>>>> (
                int id,
                [FromForm] string cocktail,
                [FromForm] IFormFile? file,
                AppDbContext db,
                IMediator mediator) =>
            {
                try
                {
                    var existingCocktail = await db.Cocktails
                        .Include(c => c.Category)
                        .FirstOrDefaultAsync(model => model.Id == id);

                    if (existingCocktail == null)
                        return TypedResults.NotFound();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    var updatedData = JsonSerializer.Deserialize<Cocktail>(cocktail, options);

                    if (updatedData == null)
                        return TypedResults.BadRequest(ResponseData<Cocktail>.Error("Invalid cocktail data"));

                    if (string.IsNullOrEmpty(updatedData.Name) || string.IsNullOrEmpty(updatedData.Description) || updatedData.Price <= 0)
                        return TypedResults.BadRequest(ResponseData<Cocktail>.Error("Invalid data after deserialization"));

                    existingCocktail.Name = updatedData.Name;
                    existingCocktail.Description = updatedData.Description;
                    existingCocktail.Price = updatedData.Price;

                    if (updatedData.Category != null && updatedData.Category.Id > 0)
                    {
                        var categoryFromDb = await db.CocktailCategories
                            .FirstOrDefaultAsync(c => c.Id == updatedData.Category.Id);

                        if (categoryFromDb != null)
                            existingCocktail.Category = categoryFromDb;
                    }
                    else
                    {
                        existingCocktail.Category = null;
                    }

                    if (file != null)
                    {
                        var imageUrl = await mediator.Send(new SaveImage(file));
                        existingCocktail.PathToPicture = imageUrl;
                        existingCocktail.MimeType = file.ContentType;
                    }

                    await db.SaveChangesAsync();

                    return TypedResults.Ok(ResponseData<Cocktail>.Success(existingCocktail));
                }
                catch (JsonException ex)
                {
                    return TypedResults.BadRequest(ResponseData<Cocktail>.Error($"JSON deserialization error: {ex.Message}"));
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest(ResponseData<Cocktail>.Error($"Error updating cocktail: {ex.Message}"));
                }
            })
            .WithName("UpdateCocktail")
            .WithOpenApi()
            .RequireAuthorization("admin");

        group.MapDelete("/{id}",
            async Task<Results<Ok<ResponseData<string>>, NotFound>> (int id, AppDbContext db) =>
            {
                var affected = await db.Cocktails
                    .Where(model => model.Id == id)
                    .ExecuteDeleteAsync();

                return affected == 1
                    ? TypedResults.Ok(ResponseData<string>.Success("Cocktail deleted successfully"))
                    : TypedResults.NotFound();
            })
            .WithName("DeleteCocktail")
            .WithOpenApi()
            .RequireAuthorization("admin");
    }
}