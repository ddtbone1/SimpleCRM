using System.ComponentModel.DataAnnotations;
namespace SimpleCRM.Models
{
    public class Agent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [StringLength(50)]
        public string Position { get; set; } = string.Empty;

        public DateTime HireDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Soft delete
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        // Navigation property for associated user account
        public User? User { get; set; }
        
        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}