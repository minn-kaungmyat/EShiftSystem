using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EShiftSystem.Models
{
    // represents a customer who can create and manage transportation jobs
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        // links customer to identity user account
        [Required]
        public required string ApplicationUserId { get; set; }

        // navigation property to the identity user
        [ForeignKey("ApplicationUserId")]
        public required ApplicationUser ApplicationUser { get; set; }

        // customer's physical address
        [Required]
        public string? Address { get; set; }

        // customer's contact phone number
        [Required]
        public string? Phone { get; set; }

        // customer's full name for identification
        [Required]
        public required string FullName { get; set; }

        // indicates if customer account is active and can create jobs
        [Required]
        public bool IsActive { get; set; } = true; // Default to active

        // collection of jobs created by this customer
        public ICollection<Job>? Jobs { get; set; }
    }
}
