using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace WEB_353502_ZGIRSKAYA.UI.Areas.Admin.Pages.Cocktails
{
    public class CreateModel : PageModel
    {
        private readonly ICocktailService _cocktailService;
        private readonly ICategoryService _categoryService;

        public CreateModel(ICocktailService cocktailService, ICategoryService categoryService)
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

        public async Task<IActionResult> OnGetAsync()
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (categoriesResponse.Successfull)
            {
                Categories = categoriesResponse.Data ?? new List<CocktailCategory>();
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

            var resp = await _cocktailService.CreateCocktailAsync(Cocktail, Image);

            if (!resp.Successfull)
            {
                ModelState.AddModelError(string.Empty, resp.ErrorMessage ?? "Ошибка при создании коктейля");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}