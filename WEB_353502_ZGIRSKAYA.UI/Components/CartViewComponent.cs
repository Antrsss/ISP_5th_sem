using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.UI.Models;

namespace WEB_353502_ZGIRSKAYA.UI.Components
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new CartViewModel
            {
                TotalPrice = "00,0 руб",
                ItemsCount = 0
            };
            return View(model);
        }
    }
}
