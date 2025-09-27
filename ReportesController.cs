using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventarioAlmacen.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos.ToListAsync();
            return View(productos);
        }

        public IActionResult Graficos()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatos(
            string categoria,
            [FromQuery] List<string> productos,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            if (categoria == "producto")
            {
                var query = _context.Productos.AsQueryable();

                if (productos != null && productos.Any())
                {
                    query = query.Where(p => productos.Contains(p.Codigo));
                }

                var data = await query
                    .Select(p => new {
                        codigo = p.Codigo,
                        label = p.Descripcion,
                        stockAlmacen = p.StockAlmacen,
                        stockProduccion = p.StockProduccion
                    })
                    .ToListAsync();

                return Json(data);
            }
            else if (categoria == "tipo")
            {
                // 🔹 Solo transferencias y devoluciones
                var query = _context.Movimientos
                    .Where(m => m.TipoMovimiento == "Transferencia" || m.TipoMovimiento == "Devolucion");

                // 🔹 Filtro por productos seleccionados
                if (productos != null && productos.Any())
                {
                    query = query.Where(m => productos.Contains(m.Codigo));
                }

                // 🔹 Si no hay fechas, usar últimos 7 días
                var desde = fechaInicio ?? DateTime.Today.AddDays(-7);
                var hasta = fechaFin ?? DateTime.Today;

                query = query.Where(m => m.FechaMovimiento >= desde && m.FechaMovimiento <= hasta);

                // 🔹 Agrupamos por producto + tipo de movimiento y sumamos
                var data = await query
                    .Join(_context.Productos,
                          m => m.Codigo,
                          p => p.Codigo,
                          (m, p) => new { m.Codigo, p.Descripcion, m.TipoMovimiento, m.Cantidad })
                    .GroupBy(x => new { x.Codigo, x.Descripcion, x.TipoMovimiento })
                    .Select(g => new {
                        codigo = g.Key.Codigo,
                        labelProducto = g.Key.Descripcion,   // ✅ nombre real del producto
                        label = g.Key.TipoMovimiento,        // ✅ tipo de movimiento (Transferencia / Devolucion)
                        valor = g.Sum(x => x.Cantidad)       // ✅ suma total en el rango
                    })
                    .ToListAsync();

                return Json(data);
            }
            else if (categoria == "produccion")
            {
                var query = _context.Movimientos
                    .Where(m => m.TipoMovimiento.ToLower().Contains("produccion"));

                // 🔹 Filtro por productos seleccionados
                if (productos != null && productos.Any())
                {
                    query = query.Where(m => productos.Contains(m.Codigo));
                }

                // 🔹 Filtro por fechas (si no hay, últimos 7 días)
                var desde = fechaInicio ?? DateTime.Today.AddDays(-7);
                var hasta = fechaFin ?? DateTime.Today;

                query = query.Where(m => m.FechaMovimiento >= desde && m.FechaMovimiento <= hasta);

                // 🔹 Traemos los datos y unimos con productos para obtener el nombre
                var datosRaw = await query
                    .Join(_context.Productos,
                          m => m.Codigo,
                          p => p.Codigo,
                          (m, p) => new { m.FechaMovimiento, m.Codigo, p.Descripcion, m.Cantidad })
                    .GroupBy(x => new { x.FechaMovimiento.Date, x.Codigo, x.Descripcion })
                    .Select(g => new {
                        Fecha = g.Key.Date,
                        Codigo = g.Key.Codigo,
                        Nombre = g.Key.Descripcion,
                        Valor = g.Sum(m => m.Cantidad)
                    })
                    .ToListAsync();

                // 🔹 Generamos todas las fechas del rango
                var todasFechas = Enumerable.Range(0, (hasta - desde).Days + 1)
                    .Select(offset => desde.AddDays(offset).Date)
                    .ToList();

                // 🔹 Obtenemos lista de productos involucrados
                var productosInvolucrados = datosRaw.Select(d => new { d.Codigo, d.Nombre })
                                                    .Distinct()
                                                    .ToList();

                // 🔹 Armamos una lista completa (rellenando con 0 cuando no hay producción en ese día)
                var data = new List<object>();
                foreach (var prod in productosInvolucrados)
                {
                    foreach (var fecha in todasFechas)
                    {
                        var encontrado = datosRaw.FirstOrDefault(d => d.Codigo == prod.Codigo && d.Fecha == fecha);
                        data.Add(new
                        {
                            label = fecha.ToString("yyyy-MM-dd"),
                            producto = prod.Nombre,
                            valor = encontrado?.Valor ?? 0
                        });
                    }
                }

                return Json(data.OrderBy(d => ((dynamic)d).label));
            }

            return Json(new { });
        }
    }
}
