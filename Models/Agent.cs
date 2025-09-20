using System.ComponentModel.DataAnnotations;
namespace SimpleCRM.Models
{
    public class Agent
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Department { get; set; }

        public string Position { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}