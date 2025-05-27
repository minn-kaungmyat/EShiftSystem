using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Container
    {
        [Key]
        public int ContainerId { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }
    }

}
