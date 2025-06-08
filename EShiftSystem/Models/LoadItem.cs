using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    public class LoadItem
    {
        [Key]
        public int LoadItemId { get; set; }

        [Required]
        public string ItemType { get; set; } // e.g., "Furniture", "Documents", "Electronics"

        public int Quantity { get; set; } = 1;

        public string? Note { get; set; }

        // FK
        public int LoadId { get; set; }
        public Load Load { get; set; }
    }

}
