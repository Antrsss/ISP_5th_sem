using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.API.UseCases;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
namespace WEB_353502_ZGIRSKAYA.API.EndPoints;

public static class CocktailEndpoints
{
    public static void MapCocktailEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Cocktail").WithTags(nameof(Cocktail));

        group.MapGet("/{category:alpha?}",
                async (IMediator mediator, string? category, int pageNo = 1, int pageSize = 3) =>
                {
                    var response = await mediator.Send(new GetListOfCocktails(category, pageNo, pageSize));
                    return response.Successfull ? Results.Ok(response) : Results.BadRequest(response);
                })
                .WithName("GetAllCocktails")
                .WithOpenApi();

       /* group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.Cocktails.ToListAsync();
        })
        .WithName("GetAllCocktails")
        .WithOpenApi();*/

        group.MapGet("/{id}", async Task<Results<Ok<Cocktail>, NotFound>> (int id, AppDbContext db) =>
        {
            return await db.Cocktails.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is Cocktail model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetCocktailById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Cocktail cocktail, AppDbContext db) =>
        {
            var affected = await db.Cocktails
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, cocktail.Id)
                    .SetProperty(m => m.Name, cocktail.Name)
                    .SetProperty(m => m.Description, cocktail.Description)
                    .SetProperty(m => m.Price, cocktail.Price)
                    .SetProperty(m => m.PathToPicture, cocktail.PathToPicture)
                    .SetProperty(m => m.MimeType, cocktail.MimeType)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateCocktail")
        .WithOpenApi();

        group.MapPost("/", async (Cocktail cocktail, AppDbContext db) =>
        {
            db.Cocktails.Add(cocktail);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Cocktail/{cocktail.Id}",cocktail);
        })
        .WithName("CreateCocktail")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, AppDbContext db) =>
        {
            var affected = await db.Cocktails
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteCocktail")
        .WithOpenApi();
    }
}
