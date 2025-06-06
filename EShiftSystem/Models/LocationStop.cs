using System.ComponentModel.DataAnnotations;
using EShiftSystem.Models;

public class LocationStop
{
    [Key]
    public int LocationStopId { get; set; }

    [Required]
    public int JobId { get; set; }
    public required Job Job { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Address { get; set; }

    // Order in the job route: 0 = Start, 1 = mid, N = End
    public int Sequence { get; set; }

    // Optional ETA or timestamp
    public DateTime? EstimatedArrivalTime { get; set; }
}
