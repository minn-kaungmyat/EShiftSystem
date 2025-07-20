using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    // represents an assistant who helps with transport operations
    public class Assistant
    {
        [Key]
        public int AssistantId { get; set; }

        // assistant's full name
        [Required, MaxLength(100)]
        public required string Name { get; set; }

        // foreign key to assigned transport unit (nullable if unassigned)
        public int? TransportUnitId { get; set; }
        
        // navigation property to assigned transport unit
        public TransportUnit? TransportUnit { get; set; }

        // indicates if the assistant is currently assigned to a transport unit
        public bool IsAssigned { get; set; } = false;
    }
}
