using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class CartController : Controller
    {
        private readonly ICocktailService _cocktailService;
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(int id, string returnUrl)
        {
            // 1. Получаем коктейль по ID
            var response = await _cocktailService.GetCocktailByIdAsync(id);

            if (!response.Successfull)
            {
                return NotFound();
            }

            // 2. Добавляем в корзину (временная реализация)
            // Здесь будет логика добавления в корзину
            // Пока просто сохраняем в сессии
            HttpContext.Session.SetInt32($"cart_{id}", 1);

            // 3. Возвращаемся на предыдущую страницу
            return LocalRedirect(returnUrl);
        }
    }
}
