using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EShiftSystem.Models
{
    public class TransportUnit
    {
        [Key]
        public int TransportUnitId { get; set; }

        public int LorryId { get; set; }
        public required Lorry Lorry { get; set; }

        public int DriverId { get; set; }
        public required Driver Driver { get; set; }

        public required ICollection<Assistant> Assistants { get; set; }

        public int ContainerId { get; set; }
        public required Container Container { get; set; }
    }

}
