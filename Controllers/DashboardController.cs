using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleCRM.Models;
using System.Security.Claims;

namespace SimpleCRM.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var isAdmin = User.IsInRole("Admin");
            var currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var dashboardData = new
            {
                TotalCustomers = CustomersController.customers.Count,
                TotalAgents = isAdmin ? AgentsController.agents.Count : 0,
                TotalTimesheetEntries = isAdmin ? 
                    TimesheetController.timesheets.Count : 
                    TimesheetController.timesheets.Count(t => t.UserId == currentUserId),
                TotalUsers = isAdmin ? AccountController.users.Count : 0,
                IsAdmin = isAdmin,
                Username = User.Identity?.Name ?? "Unknown User"
            };

            return View(dashboardData);
        }
    }
}
