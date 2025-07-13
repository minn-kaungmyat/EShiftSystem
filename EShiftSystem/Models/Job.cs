    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using EShiftSystem.Models.Enums;

    namespace EShiftSystem.Models
    {
        // represents a transportation job with customer, loads, and status tracking
        public class Job
        {
            [Key]
            public int JobId { get; set; }

            // unique job identifier in format JOB-YYYY-MM-XXXX
            [Required]
            [MaxLength(20)]
            [Display(Name = "Job Number")]
            public string JobNumber { get; set; } = string.Empty;

            // date when the job was created
            [Required]
            public DateTime JobDate { get; set; }

            // descriptive title for the job
            [Required]
            [StringLength(100)]
            public string JobTitle { get; set; } = string.Empty;

            // optional detailed description of the job
            [StringLength(500)]
            public string? Description { get; set; }

            // priority level of the job (Normal, High, Urgent)
            [Required]
            public JobPriority Priority { get; set; } = JobPriority.Normal;

        // pickup location for the entire job
        [Required]
        [MaxLength(200)]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; } = string.Empty;

        // delivery destination for the entire job
        [Required]
        [MaxLength(200)]
        [Display(Name = "Destination")]
        public string Destination { get; set; } = string.Empty;

        // Google Maps coordinates for start location
        [MaxLength(50)]
        public string? StartLatitude { get; set; }

        [MaxLength(50)]
        public string? StartLongitude { get; set; }

        // Google Maps coordinates for destination
        [MaxLength(50)]
        public string? DestinationLatitude { get; set; }

        [MaxLength(50)]
        public string? DestinationLongitude { get; set; }

        // foreign key linking job to customer
            [Required]
            [ForeignKey("Customer")]
            public int CustomerId { get; set; }

            // navigation property to customer who owns this job
        public required Customer Customer { get; set; }

            // collection of loads associated with this job
            public ICollection<Load> Loads { get; set; } = new List<Load>();

            // current status of the job (Pending, Approved, InProgress, etc.)
            public JobStatus Status { get; set; } = JobStatus.Pending;

            // timestamp when job was created
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            
            // timestamp when job was last updated
            public DateTime? UpdatedAt { get; set; }
        }
    }
