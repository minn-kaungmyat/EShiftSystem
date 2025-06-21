    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using EShiftSystem.Models.Enums;

    namespace EShiftSystem.Models
    {
        public class Job
        {
            [Key]
            public int JobId { get; set; }

            [Required]
            [MaxLength(20)]
            [Display(Name = "Job Number")]
            public string JobNumber { get; set; } = string.Empty;

            [Required]
            public DateTime JobDate { get; set; }

            [Required]
            [StringLength(100)]
            public string JobTitle { get; set; } = string.Empty;

            [StringLength(500)]
            public string? Description { get; set; }

            [Required]
            public JobPriority Priority { get; set; } = JobPriority.Normal;

        // Foreign key to Customer
            [Required]
            [ForeignKey("Customer")]
            public int CustomerId { get; set; }

            public Customer Customer { get; set; }

            // Navigation property: 1 Job has many Loads
            public ICollection<Load> Loads { get; set; } = new List<Load>();

            public JobStatus Status { get; set; } = JobStatus.Pending;

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }
        }
    }
