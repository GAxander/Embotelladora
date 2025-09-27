using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Data;
using InventarioAlmacen.Models;
using System.Threading.Tasks;
using System.Linq;

namespace InventarioAlmacen.Controllers
{
    public class DetalleProduccionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetalleProduccionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DetalleProducciones/Create
        public IActionResult Create(int produccionId)
        {
            var produccion = _context.Producciones.Find(produccionId);
            if (produccion == null) return NotFound();

            ViewBag.Productos = _context.Productos.ToList();
            ViewBag.TipoPermitidos = produccion.Linea == "SOPLADO"
                ? new[] { "PROD", "MERM", "ING1" }
                : new[] { "PROD", "MERM" };

            var detalle = new DetalleProduccion { ProduccionId = produccionId };
            return View(detalle);
        }

        // POST: DetalleProducciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetalleProduccion detalle)
        {
            var produccion = await _context.Producciones.FindAsync(detalle.ProduccionId);
            if (produccion == null) return NotFound();

            // Validación según línea
            if (produccion.Linea == "SOPLADO" && !(new[] { "PROD", "MERM", "ING1" }).Contains(detalle.TipoDetalle))
                ModelState.AddModelError("TipoDetalle", "Tipo no válido para SOPLADO");
            if (produccion.Linea == "LLENADO" && !(new[] { "PROD", "MERM" }).Contains(detalle.TipoDetalle))
                ModelState.AddModelError("TipoDetalle", "Tipo no válido para LLENADO");

            if (ModelState.IsValid)
            {
                _context.DetalleProducciones.Add(detalle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", "Producciones", new { id = detalle.ProduccionId });
            }

            ViewBag.Productos = _context.Productos.ToList();
            return View(detalle);
        }

        // GET: DetalleProducciones/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _context.DetalleProducciones
                .Include(d => d.Produccion)
                .FirstOrDefaultAsync(m => m.DetalleProduccionId == id);
            if (detalle == null) return NotFound();
            return View(detalle);
        }

        // POST: DetalleProducciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalle = await _context.DetalleProducciones.FindAsync(id);
            if (detalle != null)
            {
                _context.DetalleProducciones.Remove(detalle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Edit", "Producciones", new { id = detalle.ProduccionId });
        }
    }
}
