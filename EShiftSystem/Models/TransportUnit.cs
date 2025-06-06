using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EShiftSystem.Models
{
    public class TransportUnit
    {
        [Key]
        public int TransportUnitId { get; set; }
        [Required]
        [StringLength(100)]
        [DisplayName("Transport Unit Name")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Please select a Lorry.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Lorry.")]
        public int LorryId { get; set; }
        public Lorry? Lorry { get; set; }
        [Required(ErrorMessage = "Please select a driver.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Driver.")]
        public int DriverId { get; set; }
        public Driver? Driver { get; set; }
        public ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();
        [Required(ErrorMessage = "Please select a container.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Container.")]
        public int ContainerId { get; set; }
        public  Container? Container { get; set; }
    }

}
