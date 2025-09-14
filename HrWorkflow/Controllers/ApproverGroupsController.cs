using HrWorkflow.Data;
using HrWorkflow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Controllers
{
    public class ApproverGroupsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ApproverGroupsController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var groups = await _db.ApproverGroups
                .Include(g => g.Members)
                .ThenInclude(m => m.Employee)
                .AsNoTracking()
                .ToListAsync();
            return View(groups);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApproverGroup model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.ApproverGroups.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var group = await _db.ApproverGroups.FindAsync(id);
            if (group == null) return NotFound();
            return View(group);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApproverGroup model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var group = await _db.ApproverGroups.FindAsync(id);
            if (group == null) return NotFound();
            return View(group);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _db.ApproverGroups.FindAsync(id);
            if (group == null) return NotFound();
            _db.ApproverGroups.Remove(group);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var group = await _db.ApproverGroups
                .Include(g => g.Members)
                .ThenInclude(m => m.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            return View(group);
        }

        public async Task<IActionResult> ManageMembers(int id)
        {
            var group = await _db.ApproverGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            ViewBag.Employees = await _db.Employees.AsNoTracking().ToListAsync();
            var selected = group.Members.Select(m => m.EmployeeId).ToList();
            ViewBag.SelectedIds = selected;
            return View(group);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageMembers(int id, int[] selectedEmployeeIds)
        {
            var group = await _db.ApproverGroups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();

            var existing = group.Members.ToList();
            _db.ApproverGroupMembers.RemoveRange(existing);
            await _db.SaveChangesAsync();

            var uniqueIds = selectedEmployeeIds.Distinct().ToList();
            foreach (var empId in uniqueIds)
            {
                _db.ApproverGroupMembers.Add(new ApproverGroupMember
                {
                    ApproverGroupId = group.Id,
                    EmployeeId = empId
                });
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = group.Id });
        }
    }
}

