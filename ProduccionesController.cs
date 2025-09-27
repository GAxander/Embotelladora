using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Data;
using InventarioAlmacen.Models;

namespace InventarioAlmacen.Controllers
{
    public class ProduccionesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduccionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Producciones
        public async Task<IActionResult> Index()
        {
            var producciones = _context.Producciones
                .Include(p => p.Operador)
                .Include(p => p.Detalles);

            return View(await producciones.ToListAsync());
        }

        // GET: Producciones/Create
        public IActionResult Create()
        {
            ViewBag.Operadores = _context.Operadores.ToList();
            return View();
        }

        // POST: Producciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produccion produccion, int cantidad = 1)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produccion);
                await _context.SaveChangesAsync();

                // Generar automáticamente los detalles según la línea seleccionada
                var detalles = new List<DetalleProduccion>();

                if (produccion.Linea.ToUpper() == "SOPLADO")
                {
                    string[] tipos = { "PROD", "MERM", "ING1" };

                    foreach (var tipo in tipos)
                    {
                        detalles.Add(new DetalleProduccion
                        {
                            ProduccionId = produccion.ProduccionId,
                            TipoDetalle = tipo,
                            Codigo = "", // se completará en edición
                            Descripcion = "",
                            Cantidad = 0
                        });
                    }
                }
                else if (produccion.Linea.ToUpper() == "LLENADO")
                {
                    string[] tipos = { "PROD", "MERM" };

                    foreach (var tipo in tipos)
                    {
                        detalles.Add(new DetalleProduccion
                        {
                            ProduccionId = produccion.ProduccionId,
                            TipoDetalle = tipo,
                            Codigo = "", // se completará en edición
                            Descripcion = "",
                            Cantidad = 0
                        });
                    }
                }

                _context.DetalleProducciones.AddRange(detalles);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Edit), new { id = produccion.ProduccionId });
            }

            ViewBag.Operadores = _context.Operadores.ToList();
            return View(produccion);
        }

        // GET: Producciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var produccion = await _context.Producciones
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.ProduccionId == id);

            if (produccion == null)
                return NotFound();

            ViewBag.Productos = _context.Productos.ToList();
            ViewBag.Operadores = _context.Operadores.ToList();
            return View(produccion);
        }

        // POST: Producciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Produccion produccion)
        {
            if (id != produccion.ProduccionId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produccion);

                    // actualizar detalles
                    foreach (var detalle in produccion.Detalles)
                    {
                        if (detalle.DetalleProduccionId == 0)
                            _context.DetalleProducciones.Add(detalle);
                        else
                            _context.DetalleProducciones.Update(detalle);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduccionExists(produccion.ProduccionId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Productos = _context.Productos.ToList();
            ViewBag.Operadores = _context.Operadores.ToList();
            return View(produccion);
        }

        // GET: Producciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var produccion = await _context.Producciones
                .Include(p => p.Operador)
                .FirstOrDefaultAsync(m => m.ProduccionId == id);

            if (produccion == null)
                return NotFound();

            return View(produccion);
        }

        // POST: Producciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produccion = await _context.Producciones.FindAsync(id);
            if (produccion != null)
            {
                var detalles = _context.DetalleProducciones.Where(d => d.ProduccionId == id);
                _context.DetalleProducciones.RemoveRange(detalles);
                _context.Producciones.Remove(produccion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProduccionExists(int id)
        {
            return _context.Producciones.Any(e => e.ProduccionId == id);
        }
    }
}
