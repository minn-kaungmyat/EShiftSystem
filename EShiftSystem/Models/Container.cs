using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    // represents a container used for transporting goods
    public class Container
    {
        [Key]
        public int ContainerId { get; set; }

        // physical size of the container
        [MaxLength(50)]
        public string? Size { get; set; }

        // type/category of the container
        [MaxLength(50)]
        public string? Type { get; set; }
    }
}
