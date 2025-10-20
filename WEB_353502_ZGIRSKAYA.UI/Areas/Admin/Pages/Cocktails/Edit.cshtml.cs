using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;
using Microsoft.AspNetCore.Http;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class EditModel : PageModel
    {
        private readonly ICocktailService _cocktailService;

        public EditModel(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        [BindProperty]
        public Cocktail Cocktail { get; set; } = new Cocktail();

        [BindProperty]
        public IFormFile? Image { get; set; }

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _cocktailService.UpdateCocktailAsync(id.Value, Cocktail, Image);
                TempData["Success"] = "Коктейль успешно обновлён";
                return RedirectToPage("./Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при обновлении: {ex.Message}");
                return Page();
            }
        }
    }
}