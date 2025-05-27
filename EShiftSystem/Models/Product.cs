using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(100)]
        public required string Name { get; set; }

        public float Weight { get; set; }

        public int Quantity { get; set; }

        // Foreign key to Load
        public int LoadId { get; set; }
        public required Load Load { get; set; }
    }

}
