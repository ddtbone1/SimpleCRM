using System.ComponentModel.DataAnnotations;

namespace SimpleCRM.Models
{
    public enum UserStatus
    {
        PENDING,
        APPROVED,
        ACTIVE,
        DECLINED,
        DEACTIVATED
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User"; // "Admin" or "User"

        // Agent Information (for registration)
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        // User status workflow
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, ACTIVE, DECLINED, DEACTIVATED

        // Email verification
        public bool EmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationSentAt { get; set; }

        // Password reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Link to Agent - null for admin users who aren't agents
        public int? AgentId { get; set; }
        public Agent? Agent { get; set; }

        // Soft delete
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovalComments { get; set; }

        // Navigation properties
        public ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}