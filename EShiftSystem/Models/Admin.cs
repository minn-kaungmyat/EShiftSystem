using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EShiftSystem.Models
{
    // represents an administrator user with system management privileges
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        // links admin to identity user account
        public required string ApplicationUserId { get; set; }

        // navigation property to the identity user
        [ForeignKey("ApplicationUserId")]
        public required ApplicationUser ApplicationUser { get; set; }

        // Add any admin-specific fields here
    }
}
