using Microsoft.AspNetCore.Mvc;

namespace InventarioAlmacen.Controllers
{
    public class SeccionesController : Controller
    {
        public IActionResult Inventario()
        {
            return View();
        }

        public IActionResult Produccion()
        {
            return View();
        }

        public IActionResult ProductoTerminado()
        {
            return View();
        }
    }
}
