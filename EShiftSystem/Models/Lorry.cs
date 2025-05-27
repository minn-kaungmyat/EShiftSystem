using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Lorry
    {
        [Key]
        public int LorryId { get; set; }

        [Required, MaxLength(20)]
        public required string LicensePlate { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }
    }

}
