using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public DateTime JobDate { get; set; }

        // Foreign key to Customer
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public required Customer Customer { get; set; }

        // Navigation property: 1 Job has many Loads
        public required ICollection<Load> Loads { get; set; }
    }

}
