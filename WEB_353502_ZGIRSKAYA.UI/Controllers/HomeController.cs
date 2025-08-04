using Microsoft.AspNetCore.Mvc;

namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
