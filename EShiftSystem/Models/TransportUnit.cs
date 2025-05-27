using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EShiftSystem.Models
{
    public class TransportUnit
    {
        [Key]
        public int TransportUnitId { get; set; }

        // Foreign keys and navigation properties
        public int LorryId { get; set; }
        public required Lorry Lorry { get; set; }

        public int DriverId { get; set; }
        public required Driver Driver { get; set; }

        // One TransportUnit can have multiple assistants
        public required ICollection<Assistant> Assistants { get; set; }

        public int ContainerId { get; set; }
        public required Container Container { get; set; }
    }

}
