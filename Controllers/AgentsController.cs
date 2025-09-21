using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SimpleCRM.Models;
using SimpleCRM.Data;
using SimpleCRM.Services;

namespace SimpleCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AgentsController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IAuditService _auditService;

        public AgentsController(CrmDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var totalAgents = await _context.Agents.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalAgents / pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedAgents = await _context.Agents
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAgents = totalAgents;

            return View(paginatedAgents);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Agent agent)
        {
            if (ModelState.IsValid)
            {
                agent.CreatedDate = DateTime.Now;
                agent.IsActive = true;
                
                _context.Agents.Add(agent);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();
            return View(agent);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Agent agent)
        {
            var existing = await _context.Agents.FindAsync(agent.Id);
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                existing.Name = agent.Name;
                existing.Email = agent.Email;
                existing.Phone = agent.Phone;
                existing.Department = agent.Department;
                existing.Position = agent.Position;
                existing.IsActive = agent.IsActive;
                existing.HireDate = agent.HireDate;
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(agent);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();
            
            // Find the associated user account
            var associatedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.AgentId == id && u.DeletedAt == null);
            
            // Soft delete the agent
            agent.DeletedAt = DateTime.UtcNow;
            agent.UpdatedDate = DateTime.UtcNow;
            
            // Deactivate the associated user account if it exists
            if (associatedUser != null)
            {
                associatedUser.Status = "DEACTIVATED";
                associatedUser.DeletedAt = DateTime.UtcNow;
                associatedUser.UpdatedDate = DateTime.UtcNow;
                
                // Log the deactivation
                await _auditService.LogAsync(
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                    "DEACTIVATE_USER",
                    "User",
                    associatedUser.Id,
                    $"Deactivated user account {associatedUser.Username} due to agent deletion: {agent.Name}",
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );
            }
            
            // Log the agent deletion
            await _auditService.LogAsync(
                int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
                "DELETE_AGENT",
                "Agent",
                id,
                $"Deleted agent: {agent.Name}" + (associatedUser != null ? $" and deactivated user: {associatedUser.Username}" : ""),
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Pending Users Management
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _context.Users
                .Where(u => u.Status == "PENDING")
                .Include(u => u.Agent)
                .OrderBy(u => u.CreatedDate)
                .ToListAsync();

            return PartialView("_PendingUsers", pendingUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(int userId, string? comments = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Create Agent record automatically using user's registration information
            var newAgent = new Agent
            {
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Phone = user.Phone ?? "",
                Department = user.Department ?? "Unassigned",
                Position = user.Position ?? "Agent",
                HireDate = user.HireDate ?? DateTime.Now,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.Agents.Add(newAgent);
            await _context.SaveChangesAsync(); // Save to get the Agent ID

            // Update user to link to the new agent
            user.Status = "ACTIVE";
            user.AgentId = newAgent.Id; // Link the user to the created agent
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
                $"Approved user: {user.Username} and created agent: {newAgent.Name}",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            return Json(new { success = true, message = $"User {user.Username} has been approved and added to Agents Management successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(int userId, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return Json(new { success = false, message = "Rejection reason is required." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
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

            return Json(new { success = true, message = $"User {user.Username} has been rejected." });
        }
    }
}
