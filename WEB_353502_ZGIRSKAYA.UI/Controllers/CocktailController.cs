using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;
using WEB_353502_ZGIRSKAYA.UI.Extensions;

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
            var response = await _cocktailService.GetCocktailListAsync(category, pageNo);
            if (!response.Successfull || response.Data == null)
            {
                TempData["Error"] = response.ErrorMessage ?? "Ошибка при загрузке коктейлей";
                return View("Error");
            }

            if (!Request.IsAjaxRequest())
            {
                var categoriesResponse = await _categoryService.GetCategoryListAsync();
                if (categoriesResponse.Successfull)
                    ViewData["Categories"] = categoriesResponse.Data;
            }

            if (Request.IsAjaxRequest())
                return PartialView("_CocktailListPartial", response.Data);

            return View(response.Data);
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