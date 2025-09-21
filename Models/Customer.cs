using System.ComponentModel.DataAnnotations;

namespace SimpleCRM.Models
{
    public class Customer
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

        public bool IsActive { get; set; } = true;

        // Soft delete
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;
        
        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
