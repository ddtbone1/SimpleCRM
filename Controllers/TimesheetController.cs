using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Data;
using System.Security.Claims;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class TimesheetController : Controller
    {
        private readonly CrmDbContext _context;

        public TimesheetController(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            // Admin sees all, User sees only their own
            var query = isAdmin ? _context.Timesheets : _context.Timesheets.Where(t => t.UserId == currentUserId);
            
            var totalEntries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalEntries / pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedEntries = await query
                .Include(t => t.User)
                    .ThenInclude(u => u.Agent)
                .Include(t => t.ApprovedByUser)
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalEntries = totalEntries;
            ViewBag.IsAdmin = isAdmin;

            return View(paginatedEntries);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Timesheet timesheet)
        {
            if (ModelState.IsValid)
            {
                timesheet.UserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                timesheet.CreatedDate = DateTime.Now;
                timesheet.Status = "DRAFT";
                
                _context.Timesheets.Add(timesheet);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet == null) return NotFound();

            // Users can only edit their own entries
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!User.IsInRole("Admin") && timesheet.UserId != currentUserId)
                return Forbid();

            return View(timesheet);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Timesheet timesheet)
        {
            var existing = await _context.Timesheets.FindAsync(timesheet.Id);
            if (existing == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!User.IsInRole("Admin") && existing.UserId != currentUserId)
                return Forbid();

            if (ModelState.IsValid)
            {
                existing.Date = timesheet.Date;
                existing.HoursWorked = timesheet.HoursWorked;
                existing.Description = timesheet.Description;
                existing.ProjectName = timesheet.ProjectName;
                existing.StartTime = timesheet.StartTime;
                existing.EndTime = timesheet.EndTime;
                existing.IsBillable = timesheet.IsBillable;
                existing.Category = timesheet.Category;
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!User.IsInRole("Admin") && timesheet.UserId != currentUserId)
                return Forbid();

            _context.Timesheets.Remove(timesheet);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Approval workflow methods (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, string? comments)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            timesheet.Status = "APPROVED";
            timesheet.ApprovedByUserId = currentUserId;
            timesheet.ApprovedDate = DateTime.Now;
            timesheet.ApprovalComments = comments ?? string.Empty;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string? comments)
        {
            var timesheet = await _context.Timesheets.FindAsync(id);
            if (timesheet == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            timesheet.Status = "REJECTED";
            timesheet.ApprovedByUserId = currentUserId;
            timesheet.ApprovedDate = DateTime.Now;
            timesheet.RejectionReason = comments ?? string.Empty;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Weekly view
        public async Task<IActionResult> WeeklyView(DateTime? weekStart = null)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var startOfWeek = weekStart ?? GetStartOfWeek(DateTime.Today);
            var endOfWeek = startOfWeek.AddDays(6);

            var query = isAdmin ? _context.Timesheets : _context.Timesheets.Where(t => t.UserId == currentUserId);
            
            var weeklyEntries = await query
                .Include(t => t.User)
                    .ThenInclude(u => u.Agent)
                .Include(t => t.ApprovedByUser)
                .Where(t => t.Date >= startOfWeek && t.Date <= endOfWeek)
                .OrderBy(t => t.Date)
                .ToListAsync();

            ViewBag.WeekStart = startOfWeek;
            ViewBag.WeekEnd = endOfWeek;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.TotalHours = weeklyEntries.Sum(t => t.HoursWorked);

            return View(weeklyEntries);
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        // Bulk approval for Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkApprove(int[] timesheetIds)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            var timesheets = await _context.Timesheets
                .Where(t => timesheetIds.Contains(t.Id))
                .ToListAsync();
            
            foreach (var timesheet in timesheets)
            {
                timesheet.Status = "APPROVED";
                timesheet.ApprovedByUserId = currentUserId;
                timesheet.ApprovedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
