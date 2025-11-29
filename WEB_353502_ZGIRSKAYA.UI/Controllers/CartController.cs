using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Extensions;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    [Authorize] // Вернем авторизацию
    public class CartController : Controller
    {
        private readonly ICocktailService _cocktailService;

        public CartController(ICocktailService cocktailService)
        {
            _cocktailService = cocktailService;
        }

        public IActionResult Index()
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> Add(int id, string returnUrl)
        {
            // ВРЕМЕННО для отладки
            System.Diagnostics.Debug.WriteLine($"=== ADD METHOD CALLED: id={id} ===");

            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();

            var token = await HttpContext.GetTokenAsync("keycloak", "access_token");
            System.Diagnostics.Debug.WriteLine($"Token: {token}");


            try
            {
                var response = await _cocktailService.GetCocktailByIdAsync(id);

                System.Diagnostics.Debug.WriteLine($"API Response - Success: {response.Successfull}");
                System.Diagnostics.Debug.WriteLine($"API Response - Error: {response.ErrorMessage}");
                System.Diagnostics.Debug.WriteLine($"API Response - Data: {response.Data != null}");

                if (response.Successfull && response.Data != null)
                {
                    cart.AddToCart(response.Data);
                    HttpContext.Session.Set("cart", cart);
                    TempData["Success"] = $"{response.Data.Name} добавлен в корзину!";
                }
                else
                {
                    // Покажем конкретную ошибку
                    TempData["Error"] = $"Ошибка API: {response.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                TempData["Error"] = $"Исключение: {ex.Message}";
            }

            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Remove(int id, string returnUrl)
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart") ?? new Cart();
            cart.RemoveItems(id);
            HttpContext.Session.Set("cart", cart);
            TempData["Success"] = "Товар удален из корзины";

            return Redirect(returnUrl);
        }

        [HttpGet]
        public IActionResult Clear(string returnUrl)
        {
            HttpContext.Session.Remove("cart");
            TempData["Success"] = "Корзина очищена";
            return Redirect(returnUrl);
        }
    }
}