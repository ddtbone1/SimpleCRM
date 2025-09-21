using SimpleCRM.Models;
using SimpleCRM.Data;

namespace SimpleCRM.Services
{
    public interface IAuditService
    {
        Task LogAsync(int userId, string action, string entityType, int? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null);
        Task LogUserActionAsync(int userId, string action, string details = "", HttpContext? httpContext = null);
    }

    public class AuditService : IAuditService
    {
        private readonly CrmDbContext _context;

        public AuditService(CrmDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string action, string entityType, int? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details ?? string.Empty,
                IpAddress = ipAddress ?? string.Empty,
                UserAgent = userAgent ?? string.Empty,
                CreatedDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogUserActionAsync(int userId, string action, string details = "", HttpContext? httpContext = null)
        {
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "";
            
            await LogAsync(userId, action, "User", userId, details, ipAddress, userAgent);
        }
    }
}
