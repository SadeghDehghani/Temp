using HrWorkflow.Data;
using HrWorkflow.Models;
using HrWorkflow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWorkflowEngine _engine;
        public RequestsController(ApplicationDbContext db, IWorkflowEngine engine)
        {
            _db = db; _engine = engine;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _db.Requests
                .Include(r => r.RequestType)
                .Include(r => r.WorkflowInstance)
                .ThenInclude(i => i.CurrentStep)
                .AsNoTracking().ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateSelects();
            return View(new Request());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Request model)
        {
            if (!ModelState.IsValid) { await PopulateSelects(); return View(model); }
            model.CreatedAtUtc = DateTime.UtcNow;
            _db.Requests.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Requests
                .Include(r => r.RequestType)
                .Include(r => r.RequesterEmployee)
                .Include(r => r.WorkflowInstance)
                    .ThenInclude(i => i.StepInstances)
                    .ThenInclude(si => si.StepDefinition)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (item == null) return NotFound();
            ViewBag.AllEmployees = await _db.Employees.AsNoTracking().ToListAsync();
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> StartWorkflow(int id)
        {
            await _engine.StartWorkflowAsync(id);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Advance(int id, int instanceId, string actionName, int actorEmployeeId, string? comment)
        {
            await _engine.AdvanceAsync(instanceId, actionName, actorEmployeeId, comment);
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateSelects()
        {
            ViewBag.RequestTypes = new SelectList(await _db.RequestTypes.AsNoTracking().ToListAsync(), "Id", "Name");
            ViewBag.Employees = new SelectList(await _db.Employees.AsNoTracking().ToListAsync(), "Id", "FullName");
        }
    }
}

