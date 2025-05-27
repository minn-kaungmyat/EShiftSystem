using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        [Required, MaxLength(20)]
        public required string License { get; set; }
    }

}
