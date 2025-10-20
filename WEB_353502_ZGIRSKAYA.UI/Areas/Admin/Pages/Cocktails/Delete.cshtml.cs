using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class DeleteModel : PageModel
    {
        private readonly ICocktailService _cocktailService;

        public DeleteModel(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        [BindProperty]
        public Cocktail Cocktail { get; set; } = new Cocktail();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var resp = await _cocktailService.GetCocktailByIdAsync(id.Value);
            if (!resp.Successfull || resp.Data == null) return NotFound();

            Cocktail = resp.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                await _cocktailService.DeleteCocktailAsync(id.Value);
                TempData["Success"] = "Коктейль удалён";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}