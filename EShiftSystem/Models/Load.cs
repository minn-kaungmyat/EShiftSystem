using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Models
{
    // represents a load/shipment within a job with items and transport assignment
    public class Load
    {
        [Key]
        public int LoadId { get; set; }

        // foreign key linking load to its parent job
        [ForeignKey("Job")]
        public int JobId { get; set; }

        // navigation property to the parent job
        public Job? Job { get; set; }

        // foreign key to assigned transport unit (nullable until assigned)
        [ForeignKey("TransportUnit")]
        public int? TransportUnitId { get; set; }

        // navigation property to assigned transport unit
        public virtual TransportUnit? TransportUnit { get; set; }

        // collection of items/products in this load
        public ICollection<LoadItem> LoadItems { get; set; } = new List<LoadItem>();

        // current status of the load
        [Required]
        public JobStatus Status { get; set; } = JobStatus.Pending;

        // timestamp when load was created
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        
        // timestamp when load was last updated
        public DateTime? UpdatedAt { get; set; }
    }
}
