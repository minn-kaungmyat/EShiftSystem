using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.Models
{
    // represents an item/product within a load shipment
    public class LoadItem
    {
        [Key]
        public int LoadItemId { get; set; }

        // type/category of the item being transported (now using enum for consistency)
        [Required]
        public required string ItemType { get; set; } // e.g., "Box", "Furniture", "Electronics", "Other"

        // optional description of the item
        [StringLength(200)]
        public string? Description { get; set; }

        // quantity of this item type in the load
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        // optional estimated weight in kilograms
        [Range(0.1, 10000)]
        public decimal? WeightKg { get; set; }

        // optional special handling instructions
        [StringLength(200)]
        public string? SpecialInstructions { get; set; }

        // foreign key linking item to its load
        public int LoadId { get; set; }
        
        // navigation property to the parent load
        public required Load Load { get; set; }
    }
}
