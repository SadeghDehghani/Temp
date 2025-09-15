using System.Threading.Tasks;
using HrWorkflow.Data;
using HrWorkflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Controllers
{
    public class RequestTypesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RequestTypesController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var items = await _db.RequestTypes.AsNoTracking().ToListAsync();
            return View(items);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequestType model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.RequestTypes.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.RequestTypes.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RequestType model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.RequestTypes.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.RequestTypes.FindAsync(id);
            if (item == null) return NotFound();
            _db.RequestTypes.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.RequestTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}

