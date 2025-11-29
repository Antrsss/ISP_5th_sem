using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class IndexModel : PageModel
    {
        private readonly ICocktailService _cocktailService;

        public IndexModel(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        public ListModel<Cocktail> CocktailsModel { get; set; } = new ListModel<Cocktail>();

        public async Task<IActionResult> OnGetAsync(string? category, int pageNo = 1)
        {
            var resp = await _cocktailService.GetCocktailListAsync(category, pageNo);
            if (resp.Successfull && resp.Data != null)
            {
                CocktailsModel = resp.Data;
            }
            else
            {
                CocktailsModel = new ListModel<Cocktail>();
            }

            return Page(); // ✅ возвращаем Page() для корректного перехода
        }
    }
}
