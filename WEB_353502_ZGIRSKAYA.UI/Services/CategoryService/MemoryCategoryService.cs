using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;

namespace WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService
{
    public class MemoryCategoryService : ICategoryService
    {
        Task<ResponseData<List<CocktailCategory>>> ICategoryService.GetCategoryListAsync()
        {
            var categories = new List<CocktailCategory>
           {
               new CocktailCategory { Id=1, Name="Milk", NormilisedName="milk"},
               new CocktailCategory { Id=2, Name="Alcohol", NormilisedName="alcohol"},
               new CocktailCategory { Id=3, Name="NoMilkNoAlcohol", NormilisedName="no-milk-no-alcohol"}
           };

            var result = ResponseData<List<CocktailCategory>>.Success(categories);
            return Task.FromResult(result);
        }
    }
}
