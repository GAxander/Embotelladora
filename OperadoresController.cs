using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioAlmacen.Data;
using InventarioAlmacen.Models;
using System.Threading.Tasks;
using System.Linq;

namespace InventarioAlmacen.Controllers
{
    public class OperadoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OperadoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Operadores
        public async Task<IActionResult> Index()
        {
            return View(await _context.Operadores.ToListAsync());
        }

        // GET: Operadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var operador = await _context.Operadores
                .FirstOrDefaultAsync(m => m.OperadorId == id);

            if (operador == null) return NotFound();

            return View(operador);
        }

        // GET: Operadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Operadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OperadorId,Nombre,Area")] Operador operador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(operador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(operador);
        }

        // GET: Operadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var operador = await _context.Operadores.FindAsync(id);
            if (operador == null) return NotFound();

            return View(operador);
        }

        // POST: Operadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OperadorId,Nombre,Area")] Operador operador)
        {
            if (id != operador.OperadorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(operador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Operadores.Any(e => e.OperadorId == operador.OperadorId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(operador);
        }

        // GET: Operadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var operador = await _context.Operadores
                .FirstOrDefaultAsync(m => m.OperadorId == id);

            if (operador == null) return NotFound();

            return View(operador);
        }

        // POST: Operadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var operador = await _context.Operadores.FindAsync(id);
            _context.Operadores.Remove(operador);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
