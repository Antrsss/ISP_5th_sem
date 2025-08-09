using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.Domain.Entities;

namespace WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService
{
    public interface ICategoryService
    {
        public Task<ResponseData<List<CocktailCategory>>> GetCategoryListAsync();
    }
}
