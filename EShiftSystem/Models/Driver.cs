using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    // represents a driver who can operate transport units
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        // driver's full name
        [Required, MaxLength(100)]
        public required string Name { get; set; }

        // driver's license number for identification
        [Required, MaxLength(20)]
        public required string License { get; set; }

        // indicates if the driver is currently assigned to a transport unit
        public bool IsAssigned { get; set; } = false;
    }
}
