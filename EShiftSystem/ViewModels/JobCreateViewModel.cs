// EShiftSystem/ViewModels/JobCreateViewModel.cs
using EShiftSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.ViewModels
{
    // Renamed from ProductViewModel and properties updated
    public class LoadItemViewModel
    {
        [Required]
        [Display(Name = "Item Type")]
        public string ItemType { get; set; } // e.g., "Furniture", "Documents"

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [StringLength(200)]
        public string? Note { get; set; }
    }

    // A small ViewModel for just the Load details
    public class LoadViewModel
    {
        [Required]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; }

        [Required]
        public string Destination { get; set; }

        // Changed from List<ProductViewModel> Products
        public List<LoadItemViewModel> LoadItems { get; set; }

        public LoadViewModel()
        {
            // Always initialize the list
            // Changed from Products
            LoadItems = new List<LoadItemViewModel>();
        }
    }

    // The main ViewModel for the entire page (Structure remains the same)
    public class JobCreateViewModel
    {
        // Properties for the Job
        public int JobId { get; set; }

        [Required(ErrorMessage = "Please select a date for the job.")]
        [DataType(DataType.Date)]
        [Display(Name = "Job Date")]
        public DateTime JobDate { get; set; }

        [Required(ErrorMessage = "Please provide a job title.")]
        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public JobPriority Priority { get; set; } = JobPriority.Normal;

        public JobStatus Status { get; set; }

        // A list to hold all the load forms
        public List<LoadViewModel> Loads { get; set; }

        public JobCreateViewModel()
        {
            // Set default date to today
            JobDate = DateTime.Now;
            // Start with one empty load form ready for the user
            Loads = new List<LoadViewModel>();
        }
    }
}