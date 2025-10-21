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
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ICocktailService cocktailService, ICategoryService categoryService, ILogger<CreateModel> logger)
        {
            _cocktailService = cocktailService;
            _categoryService = categoryService;
            _logger = logger;
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
            _logger.LogInformation("=== START OnPostAsync ===");

            // Загружаем категории и находим выбранную
            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (categoriesResponse.Successfull)
            {
                Categories = categoriesResponse.Data ?? new List<CocktailCategory>();
                Cocktail.Category = Categories.FirstOrDefault(c => c.Id == SelectedCategoryId);
                _logger.LogInformation($"Selected category: {Cocktail.Category?.Name} (ID: {SelectedCategoryId})");
            }

            _logger.LogInformation($"Cocktail.Name = '{Cocktail.Name}'");
            _logger.LogInformation($"Cocktail.Description = '{Cocktail.Description}'");
            _logger.LogInformation($"Cocktail.Price = {Cocktail.Price}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Model error: {error.ErrorMessage}");
                }
                return Page();
            }

            _logger.LogInformation("Calling CreateCocktailAsync...");
            var resp = await _cocktailService.CreateCocktailAsync(Cocktail, Image);

            if (!resp.Successfull)
            {
                _logger.LogError($"Failed to create cocktail: {resp.ErrorMessage}");
                ModelState.AddModelError(string.Empty, resp.ErrorMessage ?? "Ошибка при создании коктейля");
                return Page();
            }

            _logger.LogInformation("Cocktail created successfully");
            TempData["Success"] = "Коктейль успешно создан";
            return RedirectToPage("./Index");
        }
    }
}