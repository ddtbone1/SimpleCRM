using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Data;
using SimpleCRM.Services;

namespace SimpleCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IAuditService _auditService;

        public AdminController(CrmDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public IActionResult Index()
        {
            // Redirect to main dashboard since Admin Dashboard is redundant
            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> PendingUsers()
        {
            // Redirect to Agents page where pending approvals are now handled
            return RedirectToAction("Index", "Agents");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(int userId, string? comments = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("PendingUsers");
            }

            user.Status = "ACTIVE";
            user.ApprovedByUserId = User.Identity?.Name;
            user.ApprovedDate = DateTime.UtcNow;
            user.ApprovalComments = comments ?? "Approved by admin";
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the approval
            await _auditService.LogAsync(
                int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                "APPROVE_USER",
                "User",
                userId,
                $"Approved user: {user.Username}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            TempData["Success"] = $"User {user.Username} has been approved successfully.";
            return RedirectToAction("PendingUsers");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(int userId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction("PendingUsers");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("PendingUsers");
            }

            user.Status = "DECLINED";
            user.ApprovedByUserId = User.Identity?.Name;
            user.ApprovedDate = DateTime.UtcNow;
            user.ApprovalComments = reason;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the rejection
            await _auditService.LogAsync(
                int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                "REJECT_USER",
                "User",
                userId,
                $"Rejected user: {user.Username}. Reason: {reason}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            TempData["Success"] = $"User {user.Username} has been rejected.";
            return RedirectToAction("PendingUsers");
        }

        public async Task<IActionResult> AllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Agent)
                .OrderBy(u => u.CreatedDate)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("AllUsers");
            }

            if (user.Role == "Admin")
            {
                TempData["Error"] = "Cannot deactivate admin users.";
                return RedirectToAction("AllUsers");
            }

            user.Status = "DEACTIVATED";
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the deactivation
            await _auditService.LogAsync(
                int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                "DEACTIVATE_USER",
                "User",
                userId,
                $"Deactivated user: {user.Username}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            TempData["Success"] = $"User {user.Username} has been deactivated.";
            return RedirectToAction("AllUsers");
        }

        [HttpPost]
        public async Task<IActionResult> ReactivateUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("AllUsers");
            }

            user.Status = "ACTIVE";
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log the reactivation
            await _auditService.LogAsync(
                int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                "REACTIVATE_USER",
                "User",
                userId,
                $"Reactivated user: {user.Username}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            TempData["Success"] = $"User {user.Username} has been reactivated.";
            return RedirectToAction("AllUsers");
        }

        public IActionResult Agents()
        {
            return RedirectToAction("Index", "Agents");
        }

        public IActionResult Timesheet()
        {
            return RedirectToAction("Index", "Timesheet");
        }
    }
}
