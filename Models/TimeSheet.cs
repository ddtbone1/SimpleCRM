using System.ComponentModel.DataAnnotations;

namespace SimpleCRM.Models
{
    public class Timesheet
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        [Range(0.5, 24)]
        public double HoursWorked { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string ProjectName { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public User User { get; set; }
    }
}