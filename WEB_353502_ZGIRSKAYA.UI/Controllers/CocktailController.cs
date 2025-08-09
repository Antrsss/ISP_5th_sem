using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class CocktailController : Controller
    {
        private ICocktailService _cocktailService;
        private ICategoryService _categoryService;

        public CocktailController(ICocktailService cocktailService,
            ICategoryService categoryService)
        {
            _cocktailService = cocktailService;
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index(string? category, int pageNo = 1)
        {
            var response = await _cocktailService.GetCocktailListAsync(category, pageNo);

            if (!response.Successfull)
                return NotFound(response.ErrorMessage);

            ViewData["currentCategory"] = category;
            ViewData["categories"] = (await _categoryService.GetCategoryListAsync()).Data;

            return View(response.Data);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _cocktailService.GetCocktailByIdAsync(id);
            if (!response.Successfull)
                return NotFound();
            return View(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Cocktail model, IFormFile imageFile)
        {
            await _cocktailService.UpdateCocktailAsync(id, model, imageFile);
            return RedirectToAction(nameof(Index));
        }
    }
}
