using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;
using Microsoft.AspNetCore.Http;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class EditModel : PageModel
    {
        private readonly ICocktailService _cocktailService;
        private readonly ICategoryService _categoryService;

        public EditModel(ICocktailService cocktailService, ICategoryService categoryService)
        {
            _cocktailService = cocktailService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public Cocktail Cocktail { get; set; } = new Cocktail();

        [BindProperty]
        public IFormFile? Image { get; set; }

        public List<CocktailCategory> Categories { get; set; } = new();

        [BindProperty]
        public int SelectedCategoryId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (categoriesResponse.Successfull)
            {
                Categories = categoriesResponse.Data ?? new List<CocktailCategory>();
            }

            var resp = await _cocktailService.GetCocktailByIdAsync(id);
            if (!resp.Successfull || resp.Data == null)
            {
                return NotFound();
            }

            Cocktail = resp.Data;

            if (Cocktail.Category != null)
            {
                SelectedCategoryId = Cocktail.Category.Id;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (categoriesResponse.Successfull)
            {
                Categories = categoriesResponse.Data ?? new List<CocktailCategory>();
                Cocktail.Category = Categories.FirstOrDefault(c => c.Id == SelectedCategoryId);
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _cocktailService.UpdateCocktailAsync(Cocktail.Id, Cocktail, Image);
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