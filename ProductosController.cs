using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Models;
using System;
using InventarioAlmacen.Data;

namespace InventarioEmbotelladora.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Productos.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Codigo == id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Producto producto)
        {
            if (id != producto.Codigo) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Codigo))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Codigo == id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(string id)
        {
            return _context.Productos.Any(e => e.Codigo == id);
        }

        // GET: Productos/ReporteStock
        public async Task<IActionResult> ReporteStock()
        {
            var productos = await _context.Productos.ToListAsync();
            return View(productos);
        }

        [HttpGet]
        public JsonResult BuscarProductos(string term)
        {
            var productos = _context.Productos
                .Where(p => p.Codigo.Contains(term) || p.Descripcion.Contains(term))
                .Select(p => new {
                    label = p.Codigo + " - " + p.Descripcion,
                    value = p.Descripcion,
                    codigo = p.Codigo
                })
                .Take(20) // limitar resultados
                .ToList();

            return Json(productos);
        }
    }
}
