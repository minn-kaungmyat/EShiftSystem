using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EShiftSystem.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        public required string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public required ApplicationUser ApplicationUser { get; set; }

        // Add any admin-specific fields here
    }


}
