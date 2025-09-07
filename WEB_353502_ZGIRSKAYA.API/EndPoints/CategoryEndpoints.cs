using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
namespace WEB_353502_ZGIRSKAYA.API.EndPoints;

public static class CategoryEndpoints
{
    public static void MapCocktailCategoryEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/CocktailCategory").WithTags(nameof(CocktailCategory));

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.CocktailCategories.ToListAsync();
        })
        .WithName("GetAllCocktailCategories")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<CocktailCategory>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.CocktailCategories.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is CocktailCategory model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetCocktailCategoryById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, CocktailCategory cocktailCategory, AppDbContext db) =>
        {
            var affected = await db.CocktailCategories
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, cocktailCategory.Id)
                    .SetProperty(m => m.Name, cocktailCategory.Name)
                    .SetProperty(m => m.NormilisedName, cocktailCategory.NormilisedName)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateCocktailCategory")
        .WithOpenApi();

        group.MapPost("/", async (CocktailCategory cocktailCategory, AppDbContext db) =>
        {
            db.CocktailCategories.Add(cocktailCategory);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/CocktailCategory/{cocktailCategory.Id}",cocktailCategory);
        })
        .WithName("CreateCocktailCategory")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.CocktailCategories
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteCocktailCategory")
        .WithOpenApi();
    }
}
