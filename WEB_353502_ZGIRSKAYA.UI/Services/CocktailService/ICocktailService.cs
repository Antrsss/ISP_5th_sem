using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

namespace WEB_353502_ZGIRSKAYA.UI.Services.CocktailService
{
    public interface ICocktailService
    {
        public Task<ResponseData<ListModel<Cocktail>>> GetCocktailListAsync(string?
            categoryNormalizedName, int pageNo = 1);
        public Task<ResponseData<Cocktail>> GetCocktailByIdAsync(int id);
        public Task UpdateCocktailAsync(int id, Cocktail product, IFormFile? formFile);
        public Task DeleteCocktailAsync(int id);
        public Task<ResponseData<Cocktail>> CreateCocktailAsync(Cocktail product, IFormFile? formFile);
    }
}
