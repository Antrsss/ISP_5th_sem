using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;

[Authorize]
public class CartController : Controller
{
    private readonly ICocktailService _cocktailService;
    private readonly Cart _cart;

    public CartController(ICocktailService cocktailService, Cart cart)
    {
        _cocktailService = cocktailService;
        _cart = cart;
    }

    [HttpGet]
    public async Task<IActionResult> Add(int id, string returnUrl)
    {
        var response = await _cocktailService.GetCocktailByIdAsync(id);
        if (response.Successfull && response.Data != null)
        {
            _cart.AddToCart(response.Data);
        }
        return Redirect(returnUrl);
    }

    [HttpGet]
    public IActionResult Remove(int id, string returnUrl)
    {
        _cart.RemoveItems(id);
        return Redirect(returnUrl);
    }

    [HttpGet]
    public IActionResult Clear(string returnUrl)
    {
        _cart.ClearAll();
        return Redirect(returnUrl);
    }

    public IActionResult Index()
    {
        return View(_cart);
    }
}
