using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ListDemo
{
    public int Id { get; set; }
    public string Name { get; set; }
}


namespace WEB_353502_ZGIRSKAYA.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Лабораторная работа №2";
            ViewData["ListItems"] = new List<string>
            {
                "элемент 1 списка", "элемент 2 списка"
            };

            var selectItems = new List<ListDemo>
            {
                new ListDemo { Id = 1, Name = "Item 1" },
                new ListDemo { Id = 2, Name = "Item 2" },
                new ListDemo { Id = 3, Name = "Item 3" }
            };

            return View(new SelectList(selectItems, "Id", "Name"));
        }
    }
}
