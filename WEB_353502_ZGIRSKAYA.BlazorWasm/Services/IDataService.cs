using WEB_353502_ZGIRSKAYA.BlazorWasm.Models;

namespace WEB_353502_ZGIRSKAYA.BlazorWasm.Services
{
    public interface IDataService
    {
        event Action DataLoaded;

        List<CocktailCategory> Categories { get; set; }
        List<CocktailListDto> Cocktails { get; set; }
        bool Success { get; set; }
        string ErrorMessage { get; set; }
        int TotalPages { get; set; }
        int CurrentPage { get; set; }
        CocktailCategory SelectedCategory { get; set; }

        Task GetProductListAsync(int pageNo = 1);
        Task GetCategoryListAsync();
        Task<CocktailDetailsDto?> GetCocktailDetailsAsync(int id);
    }
}