using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class DetailsModel : PageModel
    {
        private readonly ICocktailService _cocktailService;

        public DetailsModel(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        public Cocktail Cocktail { get; set; } = new Cocktail();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var resp = await _cocktailService.GetCocktailByIdAsync(id.Value);
            if (!resp.Successfull || resp.Data == null)
            {
                TempData["Error"] = resp.ErrorMessage ?? "Коктейль не найден";
                return NotFound();
            }

            Cocktail = resp.Data;
            return Page();
        }
    }
}