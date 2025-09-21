using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCRM.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        [Required]
        public int TimesheetId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.1, 24.0, ErrorMessage = "Hours must be between 0.1 and 24")]
        public double Hours { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public bool IsBillable { get; set; } = true;

        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        // Soft delete
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("TimesheetId")]
        public virtual Timesheet? Timesheet { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
