using Microsoft.AspNetCore.Mvc;

namespace InventarioAlmacen.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

