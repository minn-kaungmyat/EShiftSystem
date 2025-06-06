using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Models
{
    public class Load
    {
        [Key]
        public int LoadId { get; set; }

        // Foreign key to Job
        [ForeignKey("Job")]
        public int JobId { get; set; }

        public Job? Job { get; set; }

        [Required, MaxLength(100)]
        public required string StartLocation { get; set; }

        [Required, MaxLength(100)]
        public required string Destination { get; set; }

        // FK to TransportUnit
        [ForeignKey("TransportUnit")]
        public int TransportUnitId { get; set; }

        public TransportUnit TransportUnit { get; set; }

        // Navigation property: 1 Load can have many Products
        public  ICollection<Product> Products { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

}
