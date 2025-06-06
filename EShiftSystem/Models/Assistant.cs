using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Assistant
    {
        [Key]
        public int AssistantId { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        // Foreign key to TransportUnit for one-to-many relation
        public int? TransportUnitId { get; set; }
        public TransportUnit? TransportUnit { get; set; }
    }

}
