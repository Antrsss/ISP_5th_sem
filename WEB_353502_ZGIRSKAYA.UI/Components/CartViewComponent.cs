using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Models;

public class CartViewComponent : ViewComponent
{
    private readonly Cart _cart;

    public CartViewComponent(Cart cart)
    {
        _cart = cart;
    }

    public IViewComponentResult Invoke()
    {
        var model = new CartViewModel
        {
            TotalPrice = _cart.TotalPrice.ToString("C"),
            ItemsCount = _cart.Count
        };
        return View(model);
    }
}
