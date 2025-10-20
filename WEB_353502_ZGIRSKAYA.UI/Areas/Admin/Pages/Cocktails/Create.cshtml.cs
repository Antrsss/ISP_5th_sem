using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;
using Microsoft.AspNetCore.Http;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class CreateModel : PageModel
    {
        private readonly ICocktailService _cocktailService;

        public CreateModel(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        [BindProperty]
        public Cocktail Cocktail { get; set; } = new Cocktail();

        [BindProperty]
        public IFormFile? Image { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resp = await _cocktailService.CreateCocktailAsync(Cocktail, Image);
            if (!resp.Successfull)
            {
                ModelState.AddModelError(string.Empty, resp.ErrorMessage ?? "Ошибка при создании коктейля");
                return Page();
            }

            TempData["Success"] = "Коктейль успешно создан";
            return RedirectToPage("./Index");
        }
    }
}