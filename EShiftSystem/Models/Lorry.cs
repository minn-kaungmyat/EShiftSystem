using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    // represents a lorry/truck vehicle used for transportation
    public class Lorry
    {
        [Key]
        public int LorryId { get; set; }

        // vehicle's license plate number
        [Required, MaxLength(20)]
        public required string LicensePlate { get; set; }

        // vehicle model information
        [MaxLength(50)]
        public string? Model { get; set; }
    }
}
