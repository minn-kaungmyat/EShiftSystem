using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShiftSystem.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public DateTime JobDate { get; set; }

        [Required]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public JobPriority Priority { get; set; } = JobPriority.Normal;

        // Foreign key to Customer
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public required Customer Customer { get; set; }

        // Navigation property: 1 Job has many Loads
        public ICollection<Load> Loads { get; set; } = new List<Load>();
    }
}
