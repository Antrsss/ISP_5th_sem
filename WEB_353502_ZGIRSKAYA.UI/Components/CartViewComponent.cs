using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Extensions;
using WEB_353502_ZGIRSKAYA.UI.Models;

namespace WEB_353502_ZGIRSKAYA.UI.Components
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            Cart cart = HttpContext.Session.Get<Cart>("cart");

            var model = new CartViewModel
            {
                TotalPrice = cart?.TotalPrice.ToString("C") ?? "0,00 ₽",
                ItemsCount = cart?.Count ?? 0
            };

            return View(model);
        }
    }
}