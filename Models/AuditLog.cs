using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCRM.Models
{
    public enum AuditAction
    {
        LOGIN,
        LOGOUT,
        SIGNUP,
        EMAIL_VERIFIED,
        PASSWORD_RESET,
        ROLE_CHANGED,
        STATUS_CHANGED,
        USER_APPROVED,
        USER_DECLINED,
        TIMESHEET_CREATED,
        TIMESHEET_UPDATED,
        TIMESHEET_SUBMITTED,
        TIMESHEET_APPROVED,
        TIMESHEET_REJECTED,
        TIMESHEET_LOCKED,
        CUSTOMER_CREATED,
        CUSTOMER_UPDATED,
        CUSTOMER_DELETED,
        AGENT_CREATED,
        AGENT_UPDATED,
        AGENT_DEACTIVATED
    }

    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // LOGIN, SIGNUP, ROLE_CHANGE, etc.

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty; // User, Timesheet, Customer, etc.

        public int? EntityId { get; set; }

        [StringLength(1000)]
        public string Details { get; set; } = string.Empty; // JSON or description

        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
