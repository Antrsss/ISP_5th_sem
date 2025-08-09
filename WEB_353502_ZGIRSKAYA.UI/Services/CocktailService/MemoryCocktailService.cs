using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;

namespace WEB_353502_ZGIRSKAYA.UI.Services.CocktailService
{
    public class MemoryCocktailService : ICocktailService
    {
        private List<Cocktail> _cocktails;
        private List<CocktailCategory> _categories;
        private readonly IConfiguration _configuration;

        public MemoryCocktailService(
            [FromServices] IConfiguration config, 
            ICategoryService categoryService)
        {
            _categories = categoryService.GetCategoryListAsync()
                .Result
                .Data;
            _cocktails = new List<Cocktail>();
            _configuration = config;

            SetupData();
        }

        private void SetupData()
        {
            _cocktails.AddRange(new List<Cocktail>
            {
                new Cocktail
                {
                    Id = 2,
                    Name = "Mojito",
                    Description = "Refreshing mint cocktail",
                    PathToPicture = "/images/cocktails/mojito.jpg",
                    Price = 7.50,
                    Category = _categories.Find(c => c.NormilisedName == "alcohol")
                },
                new Cocktail
                {
                    Id = 3,
                    Name = "Virgin Colada",
                    Description = "Non-alcoholic tropical drink",
                    PathToPicture = "/images/cocktails/colada.jpg",
                    Price = 6.00,
                    Category = _categories.Find(c => c.NormilisedName == "no-milk-no-alcohol")
                },
                new Cocktail
                {
                    Id = 4,
                    Name = "Grasshopper",
                    Description = "Mint chocolate dessert cocktail",
                    PathToPicture = "/images/cocktails/grasshopper.jpg",
                    Price = 8.25,
                    Category = _categories.Find(c => c.NormilisedName == "milk")
                },
                new Cocktail
                {
                    Id = 5,
                    Name = "Martini",
                    Description = "Classic gin cocktail",
                    PathToPicture = "/images/cocktails/martini.jpg",
                    Price = 9.75,
                    Category = _categories.Find(c => c.NormilisedName == "alcohol")
                }
            });
        }
        public Task<ResponseData<Cocktail>> CreateCocktailAsync(Cocktail product, IFormFile? formFile)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCocktailAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseData<Cocktail>> GetCocktailByIdAsync(int id)
        {
            var cocktail = _cocktails.FirstOrDefault(c => c.Id == id);

            var result = new ResponseData<Cocktail>
            {
                Data = cocktail,
                Successfull = cocktail != null,
                ErrorMessage = cocktail == null ? "Cocktail not found" : null
            };

            return Task.FromResult(result);
        }

        public Task<ResponseData<ListModel<Cocktail>>> GetCocktailListAsync(
            string? categoryNormalizedName, int pageNo = 1)
        {
            var pageSize = _configuration.GetValue<int>("PageSettings:PageSize", 3);

            var filteredCocktails = _cocktails
                .Where(c => categoryNormalizedName == null ||
                      (c.Category != null && c.Category.NormilisedName == categoryNormalizedName))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)filteredCocktails.Count / pageSize);

            pageNo = Math.Max(1, Math.Min(pageNo, totalPages));

            var pagedCocktails = filteredCocktails
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new ResponseData<ListModel<Cocktail>>
            {
                Data = new ListModel<Cocktail>
                {
                    Items = pagedCocktails,
                    CurrentPage = pageNo,
                    TotalPages = totalPages,
                    PageSize = pageSize
                },
                Successfull = true
            };

            return Task.FromResult(result);
        }

        public Task UpdateCocktailAsync(int id, Cocktail product, IFormFile? formFile)
        {
            throw new NotImplementedException();
        }
    }
}
