using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Data;
using InventarioAlmacen.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using InventarioAlmacen.ViewModels;

namespace InventarioAlmacen.Controllers
{
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovimientosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movimientos
        

        // GET: Movimientos/Create
        public IActionResult Create()
        {
            ViewBag.Productos = _context.Productos.OrderBy(p => p.Descripcion).ToList();
            return View();
        }

        // POST: Movimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,TipoMovimiento,Cantidad,NumeroDocumento,Observacion")] Movimiento movimiento)
        {
            Console.WriteLine("====== POST /Movimientos/Create ======");
            Console.WriteLine($"Codigo: {movimiento.Codigo}");
            Console.WriteLine($"TipoMovimiento: {movimiento.TipoMovimiento}");
            Console.WriteLine($"Cantidad: {movimiento.Cantidad}");
            Console.WriteLine($"NumeroDocumento: {movimiento.NumeroDocumento}");
            Console.WriteLine($"Observacion: {movimiento.Observacion}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState válido ✅");
                if (movimiento.FechaMovimiento == default(DateTime))
                {
                    movimiento.FechaMovimiento = DateTime.Today;
                }

                try
                {
                    _context.Movimientos.Add(movimiento);

                    // actualizar stock
                    var producto = await _context.Productos.FindAsync(movimiento.Codigo);
                    if (producto != null)
                    {
                        switch (movimiento.TipoMovimiento)
                        {
                            case "Ingreso":
                                producto.StockAlmacen += movimiento.Cantidad;
                                break;
                            case "Consumo":
                            case "Merma":
                                producto.StockProduccion -= movimiento.Cantidad;
                                break;
                            case "Transferencia":
                                producto.StockAlmacen -= movimiento.Cantidad;
                                producto.StockProduccion += movimiento.Cantidad;
                                break;
                            case "Devolucion":
                                producto.StockAlmacen += movimiento.Cantidad;
                                producto.StockProduccion -= movimiento.Cantidad;
                                break;
                        }

                        _context.Update(producto);
                    }
                    else
                    {
                        Console.WriteLine("⚠ Producto no encontrado en la BD con el código enviado");
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine("Movimiento guardado correctamente ✅");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al guardar: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState inválido, no se guarda");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error de validación: {error.ErrorMessage}");
                }
            }

            ViewBag.Productos = _context.Productos.OrderBy(p => p.Descripcion).ToList();
            return View(movimiento);
        }

        public async Task<IActionResult> Index(
    string? codigo,
    string? nombre,
    string? tipoMovimiento,
    DateTime? fechaDesde,
    DateTime? fechaHasta,
    string? numeroDocumento)
        {
            var movimientos = _context.Movimientos
                .Include(m => m.Producto)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(codigo))
                movimientos = movimientos.Where(m => m.Codigo == codigo);

            if (!string.IsNullOrEmpty(nombre))
                movimientos = movimientos.Where(m => m.Producto.Descripcion.Contains(nombre));

            if (!string.IsNullOrEmpty(tipoMovimiento))
                movimientos = movimientos.Where(m => m.TipoMovimiento == tipoMovimiento);

            if (fechaDesde.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                movimientos = movimientos.Where(m => m.FechaMovimiento <= fechaHasta.Value);

            if (!string.IsNullOrEmpty(numeroDocumento))
                movimientos = movimientos.Where(m => m.NumeroDocumento.Contains(numeroDocumento));

            var lista = await movimientos
                .OrderBy(m => m.FechaMovimiento)
                .ToListAsync();

            // ==== Calcular stocks antes y después ====
            foreach (var mov in lista)
            {
                if (mov.Producto == null) continue;

                var stockAlmacenActual = mov.Producto.StockAlmacen;
                var stockProduccionActual = mov.Producto.StockProduccion;

                switch (mov.TipoMovimiento?.Trim().ToUpperInvariant())
                {
                    case "INGRESO":
                        mov.StockAlmacenDespues = stockAlmacenActual;
                        mov.StockAlmacenAntes = stockAlmacenActual - mov.Cantidad;
                        mov.StockProduccionAntes = mov.StockProduccionDespues = stockProduccionActual;
                        break;

                    case "TRANSFERENCIA":
                        mov.StockAlmacenDespues = stockAlmacenActual;
                        mov.StockAlmacenAntes = stockAlmacenActual + mov.Cantidad;

                        mov.StockProduccionDespues = stockProduccionActual;
                        mov.StockProduccionAntes = stockProduccionActual - mov.Cantidad;
                        break;

                    case "DEVOLUCION":
                        mov.StockAlmacenDespues = stockAlmacenActual;
                        mov.StockAlmacenAntes = stockAlmacenActual - mov.Cantidad;

                        mov.StockProduccionDespues = stockProduccionActual;
                        mov.StockProduccionAntes = stockProduccionActual + mov.Cantidad;
                        break;

                    case "MERMA":
                    case "PRODUCCION":
                        mov.StockProduccionDespues = stockProduccionActual;
                        mov.StockProduccionAntes = stockProduccionActual + mov.Cantidad;
                        mov.StockAlmacenAntes = mov.StockAlmacenDespues = stockAlmacenActual;
                        break;
                }
            }

            ViewData["TipoMovimientos"] = new SelectList(
                new List<string> { "Ingreso", "Transferencia", "Devolucion", "Merma", "Produccion" }
            );

            return View(lista);
        }


        // GET: Movimientos/Edit/5
        // GET: Movimientos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            // ✅ Combo de productos (selecciona el producto actual)
            ViewBag.Codigo = new SelectList(_context.Productos, "Codigo", "Descripcion", movimiento.Codigo);

            // ✅ Combo de tipos de movimiento (selecciona el tipo actual)
            ViewBag.TipoMovimientos = new SelectList(new List<string>
    {
        "Ingreso", "Transferencia", "Devolucion", "Merma", "Produccion"
    }, movimiento.TipoMovimiento);

            return View(movimiento);
        }


        // POST: Movimientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movimiento movimiento)
        {
            if (id != movimiento.MovimientoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el movimiento original desde BD
                    var movimientoOriginal = await _context.Movimientos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.MovimientoId == id);

                    if (movimientoOriginal == null)
                        return NotFound();

                    // Producto original
                    var productoOriginal = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Codigo == movimientoOriginal.Codigo);

                    // Producto nuevo
                    var productoNuevo = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Codigo == movimiento.Codigo);

                    if (productoOriginal == null || productoNuevo == null)
                        return NotFound();

                    // 1. Revertir efecto del movimiento original sobre el producto original
                    AjustarStock(productoOriginal, movimientoOriginal, revertir: true);

                    // 2. Aplicar el nuevo movimiento sobre el producto nuevo
                    AjustarStock(productoNuevo, movimiento, revertir: false);

                    // 🔹 Muy importante: marcar ambos productos como modificados
                    _context.Update(productoOriginal);
                    _context.Update(productoNuevo);

                    // 🔹 Actualizar el movimiento en la tabla Movimientos
                    _context.Update(movimiento);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovimientoExists(movimiento.MovimientoId))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(movimiento);
        }



        // GET: Movimientos/Delete/5
        // GET: Movimientos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.Movimientos
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.MovimientoId == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // POST: Movimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);

            if (movimiento != null)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Codigo == movimiento.Codigo);

                if (producto != null)
                {
                    // 🔹 Revertir el efecto del movimiento antes de eliminarlo
                    AjustarStock(producto, movimiento, revertir: true);
                }

                _context.Movimientos.Remove(movimiento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        private void AjustarStock(Producto producto, Movimiento movimiento, bool revertir)
        {
            var factor = revertir ? -1 : 1;
            var tipo = (movimiento.TipoMovimiento ?? "").Trim().ToUpperInvariant();

            switch (tipo)
            {
                case "INGRESO":
                    producto.StockAlmacen += factor * movimiento.Cantidad;
                    break;

                case "TRANSFERENCIA":
                    producto.StockAlmacen -= factor * movimiento.Cantidad;
                    producto.StockProduccion += factor * movimiento.Cantidad;
                    break;

                case "DEVOLUCION":
                    producto.StockProduccion -= factor * movimiento.Cantidad;
                    producto.StockAlmacen += factor * movimiento.Cantidad;
                    break;

                case "MERMA":
                    producto.StockProduccion -= factor * movimiento.Cantidad;
                    break;

                case "PRODUCCION":
                    producto.StockProduccion -= factor * movimiento.Cantidad;
                    break;
            }
        }

        private bool MovimientoExists(int id)
        {
            return _context.Movimientos.Any(e => e.MovimientoId == id);
        }

        // GET: Movimientos/CreateMultiple
        // GET: Movimientos/CreateMultiple?cantidad=1
        public IActionResult CreateMultiple(int cantidad = 1)
        {
            if (cantidad < 1) cantidad = 1;

            var movimientos = new List<Movimiento>();
            for (int i = 0; i < cantidad; i++)
            {
                movimientos.Add(new Movimiento { FechaMovimiento = DateTime.Now });
            }

            ViewBag.Cantidad = cantidad;
            ViewBag.Productos = _context.Productos.OrderBy(p => p.Descripcion).ToList();
            return View(movimientos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(List<Movimiento> movimientos)
        {
            if (movimientos == null || !movimientos.Any())
            {
                ModelState.AddModelError("", "Debe ingresar al menos un movimiento.");
                ViewBag.Productos = _context.Productos.OrderBy(p => p.Descripcion).ToList();
                return View(new List<Movimiento> { new Movimiento { FechaMovimiento = DateTime.Now } });
            }

            foreach (var mov in movimientos)
            {
                if (string.IsNullOrWhiteSpace(mov.Codigo)) continue;

                var producto = await _context.Productos.FindAsync(mov.Codigo);
                if (producto != null)
                {
                    switch (mov.TipoMovimiento)
                    {
                        case "Ingreso":
                            producto.StockAlmacen += mov.Cantidad;
                            break;
                        case "Transferencia":
                            producto.StockAlmacen -= mov.Cantidad;
                            producto.StockProduccion += mov.Cantidad;
                            break;
                        case "Devolucion":
                            producto.StockAlmacen += mov.Cantidad;
                            producto.StockProduccion -= mov.Cantidad;
                            break;
                        case "Merma":
                            producto.StockProduccion -= mov.Cantidad;
                            break;
                        case "Produccion":
                            producto.StockProduccion -= mov.Cantidad;
                            break;
                    }

                    _context.Movimientos.Add(mov);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




    }
}
