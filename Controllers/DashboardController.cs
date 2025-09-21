using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Data;
using System.Security.Claims;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CrmDbContext _context;

        public DashboardController(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var dashboardData = new
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalAgents = isAdmin ? await _context.Agents.CountAsync() : 0,
                TotalTimesheetEntries = isAdmin ? 
                    await _context.Timesheets.CountAsync() : 
                    await _context.Timesheets.CountAsync(t => t.UserId == currentUserId),
                TotalUsers = isAdmin ? await _context.Users.CountAsync() : 0,
                IsAdmin = isAdmin,
                Username = User.Identity?.Name ?? "Unknown User"
            };

            return View(dashboardData);
        }
    }
}
