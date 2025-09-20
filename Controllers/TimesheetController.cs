using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleCRM.Models;
using System.Security.Claims;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class TimesheetController : Controller
    {
        public static List<Timesheet> timesheets = new List<Timesheet>();
        private static int nextId = 1;

        static TimesheetController()
        {
            // Add mock timesheet entries
            for (int i = 1; i <= 30; i++)
            {
                timesheets.Add(new Timesheet
                {
                    Id = nextId++,
                    UserId = Random.Shared.Next(1, 3), // Random user 1 or 2
                    Date = DateTime.Now.AddDays(-Random.Shared.Next(0, 30)),
                    HoursWorked = Random.Shared.Next(4, 9),
                    Description = $"Work task {i}",
                    ProjectName = $"Project {Random.Shared.Next(1, 5)}"
                });
            }
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            // Admin sees all, User sees only their own
            var userTimesheets = isAdmin ? timesheets : timesheets.Where(t => t.UserId == currentUserId);
            
            var totalEntries = userTimesheets.Count();
            var totalPages = (int)Math.Ceiling((double)totalEntries / pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedEntries = userTimesheets
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

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
        public IActionResult Create(Timesheet timesheet)
        {
            if (ModelState.IsValid)
            {
                timesheet.Id = nextId++;
                timesheet.UserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                timesheets.Add(timesheet);
                return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        public IActionResult Edit(int id)
        {
            var timesheet = timesheets.FirstOrDefault(t => t.Id == id);
            if (timesheet == null) return NotFound();

            // Users can only edit their own entries
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!User.IsInRole("Admin") && timesheet.UserId != currentUserId)
                return Forbid();

            return View(timesheet);
        }

        [HttpPost]
        public IActionResult Edit(Timesheet timesheet)
        {
            var existing = timesheets.FirstOrDefault(t => t.Id == timesheet.Id);
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
                return RedirectToAction("Index");
            }
            return View(timesheet);
        }

        public IActionResult Delete(int id)
        {
            var timesheet = timesheets.FirstOrDefault(t => t.Id == id);
            if (timesheet == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!User.IsInRole("Admin") && timesheet.UserId != currentUserId)
                return Forbid();

            timesheets.Remove(timesheet);
            return RedirectToAction("Index");
        }
    }
}
