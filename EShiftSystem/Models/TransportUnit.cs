using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using EShiftSystem.Models.Enums;

namespace EShiftSystem.Models
{
    // represents a complete transport unit with lorry, driver, assistants and container
    public class TransportUnit
    {
        [Key]
        public int TransportUnitId { get; set; }
        
        // descriptive name for the transport unit
        [Required]
        [StringLength(100)]
        [DisplayName("Transport Unit Name")]
        public required string Name { get; set; }

        // foreign key to the assigned lorry
        [Required(ErrorMessage = "Please select a Lorry.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Lorry.")]
        public int LorryId { get; set; }
        
        // navigation property to the lorry
        public Lorry? Lorry { get; set; }
        
        // foreign key to the assigned driver
        [Required(ErrorMessage = "Please select a driver.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Driver.")]
        public int DriverId { get; set; }
        
        // navigation property to the driver
        public Driver? Driver { get; set; }
        
        // collection of assistants assigned to this transport unit
        public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
        
        // foreign key to the assigned container
        [Required(ErrorMessage = "Please select a container.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Container.")]
        public int ContainerId { get; set; }
        
        // navigation property to the container
        public  Container? Container { get; set; }
        
        // current operational status of the transport unit
        [DisplayName("Status")]
        public TransportUnitStatus Status { get; set; } = TransportUnitStatus.Available;

        // collection of loads assigned to this transport unit
        public virtual ICollection<Load> Loads { get; set; } = new List<Load>();
    }

}
