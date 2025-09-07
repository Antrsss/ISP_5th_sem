using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class CocktailController : Controller
    {
        private readonly ICocktailService _cocktailService;
        private readonly ICategoryService _categoryService;

        public CocktailController(ICocktailService cocktailService, ICategoryService categoryService)
        {
            _cocktailService = cocktailService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? category, int pageNo = 1)
        {
            try
            {
                var cocktailResponse = await _cocktailService.GetCocktailListAsync(category, pageNo);

                if (!cocktailResponse.Successfull)
                {
                    TempData["Error"] = cocktailResponse.ErrorMessage;
                    return View(new ListModel<Cocktail>());
                }

                // Исправленная загрузка категорий
                var categoryResponse = await _categoryService.GetCategoryListAsync();
                var categories = categoryResponse.Successfull ? categoryResponse.Data : new List<CocktailCategory>();

                ViewData["CurrentCategory"] = category;
                ViewData["Categories"] = categories; // Убедитесь, что используете тот же ключ, что и в View
                ViewData["CurrentPage"] = pageNo;

                return View(cocktailResponse.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке данных: {ex.Message}";
                return View(new ListModel<Cocktail>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _cocktailService.GetCocktailByIdAsync(id);

                if (!response.Successfull)
                {
                    TempData["Error"] = response.ErrorMessage;
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке коктейля: {ex.Message}";
                return NotFound();
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _cocktailService.GetCocktailByIdAsync(id);

                if (!response.Successfull)
                {
                    TempData["Error"] = response.ErrorMessage;
                    return NotFound();
                }

                await LoadCategories();
                return View(response.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке данных: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cocktail model, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return View(model);
                }

                await _cocktailService.UpdateCocktailAsync(id, model, imageFile);
                TempData["Success"] = "Коктейль успешно обновлен";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при обновлении: {ex.Message}";
                await LoadCategories();
                return View(model);
            }
        }

        public IActionResult Create()
        {
            try
            {
                LoadCategories().Wait(); // Для синхронного вызова
                return View(new Cocktail());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке данных: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cocktail model, IFormFile? imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadCategories();
                    return View(model);
                }

                var response = await _cocktailService.CreateCocktailAsync(model, imageFile);

                if (!response.Successfull)
                {
                    TempData["Error"] = response.ErrorMessage;
                    await LoadCategories();
                    return View(model);
                }

                TempData["Success"] = "Коктейль успешно создан";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при создании: {ex.Message}";
                await LoadCategories();
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _cocktailService.GetCocktailByIdAsync(id);

                if (!response.Successfull)
                {
                    TempData["Error"] = response.ErrorMessage;
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке данных: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _cocktailService.DeleteCocktailAsync(id);
                TempData["Success"] = "Коктейль успешно удален";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadCategories()
        {
            var categoryResponse = await _categoryService.GetCategoryListAsync();
            if (categoryResponse.Successfull)
            {
                ViewData["Categories"] = categoryResponse.Data;
            }
            else
            {
                ViewData["Categories"] = new List<CocktailCategory>();
                TempData["Error"] = "Не удалось загрузить категории";
            }
        }
    }
}