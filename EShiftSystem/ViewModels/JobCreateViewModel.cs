// EShiftSystem/ViewModels/JobCreateViewModel.cs
using EShiftSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.ViewModels
{
    // view model for representing individual items within a load
    public class LoadItemViewModel
    {
        // type/category of the item being transported
        [Required]
        [Display(Name = "Item Type")]
        public required string ItemType { get; set; } // e.g., "Box", "Furniture", "Electronics", "Other"

        // optional description of the item
        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // quantity of this item type
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        // optional estimated weight in kilograms
        [Range(0.1, 10000)]
        [Display(Name = "Weight (kg)")]
        public decimal? WeightKg { get; set; }

        // optional special handling instructions
        [StringLength(200)]
        [Display(Name = "Special Instructions")]
        public string? SpecialInstructions { get; set; }


    }

    // view model for representing a single load with items
    public class LoadViewModel
    {
        // collection of items in this load
        public List<LoadItemViewModel> LoadItems { get; set; }

        public LoadViewModel()
        {
            // initialize empty items list
            LoadItems = new List<LoadItemViewModel>();
        }
    }

    // main view model for creating new jobs with multiple loads
    public class JobCreateViewModel
    {
        // job identifier for editing existing jobs
        public int JobId { get; set; }

        // date when the job should be executed
        [Required(ErrorMessage = "Please select a date for the job.")]
        [DataType(DataType.Date)]
        [Display(Name = "Job Date")]
        public DateTime JobDate { get; set; }

        // descriptive title for the job
        [Required(ErrorMessage = "Please provide a job title.")]
        [StringLength(100)]
        [Display(Name = "Job Title")]
        public required string JobTitle { get; set; }

        // optional detailed description
        [StringLength(500)]
        public string? Description { get; set; }

        // priority level of the job
        [Required]
        public JobPriority Priority { get; set; } = JobPriority.Normal;

        // pickup location for the entire job
        [Required(ErrorMessage = "Please provide a start location.")]
        [StringLength(200)]
        [Display(Name = "Start Location")]
        public required string StartLocation { get; set; }

        // delivery destination for the entire job
        [Required(ErrorMessage = "Please provide a destination.")]
        [StringLength(200)]
        [Display(Name = "Destination")]
        public required string Destination { get; set; }

        // Google Maps coordinates for start location
        [StringLength(50)]
        public string? StartLatitude { get; set; }

        [StringLength(50)]
        public string? StartLongitude { get; set; }

        // Google Maps coordinates for destination
        [StringLength(50)]
        public string? DestinationLatitude { get; set; }

        [StringLength(50)]
        public string? DestinationLongitude { get; set; }

        // current status of the job
        public JobStatus Status { get; set; }

        // collection of loads associated with this job
        public List<LoadViewModel> Loads { get; set; }

        public JobCreateViewModel()
        {
            // set default date to today
            JobDate = DateTime.Now;
            // initialize empty loads collection
            Loads = new List<LoadViewModel>();
        }
    }
}