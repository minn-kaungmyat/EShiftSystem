using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EShiftSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public required string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        public required ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        public required string FullName { get; set; }

        public ICollection<Job>? Jobs { get; set; }
    }



}
