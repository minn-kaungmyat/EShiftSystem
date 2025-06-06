// EShiftSystem/ViewModels/JobCreateViewModel.cs
using EShiftSystem.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace EShiftSystem.ViewModels
{
    public class ProductViewModel
    {
        [Required]
        [Display(Name = "Product Name")]
        public string Name { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, 10000)]
        public float Weight { get; set; }
    }
    // A small ViewModel for just the Load details
    public class LoadViewModel
    {
        [Required]
        [Display(Name = "Start Location")]
        public string StartLocation { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required(ErrorMessage = "Please select a transport unit.")]
        [Display(Name = "Transport Unit")]
        public int TransportUnitId { get; set; }
        public List<ProductViewModel> Products { get; set; }

        public LoadViewModel()
        {
            // Always initialize the list
            Products = new List<ProductViewModel>();
        }
    }

    // The main ViewModel for the entire page
    public class JobCreateViewModel
    {
        // Properties for the Job
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

        // A list to hold all the load forms
        public List<LoadViewModel> Loads { get; set; }

        public JobCreateViewModel()
        {
            // Start with one empty load form ready for the user
            Loads = new List<LoadViewModel>();
        }
    }
}