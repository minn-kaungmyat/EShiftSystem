namespace EShiftSystem.Models.Enums
{
    public enum JobStatus
    {
        Pending,            // Job booked by customer, awaiting admin review
        Approved,           // Admin has approved the job
        Rejected,           // Admin rejected the job (optional reason field)
        InProgress,         // At least one load is being handled (e.g., driver assigned, on the way)
        Delivered,          // All loads have been delivered by transport units
        Completed,          // All loads confirmed as received/completed by customer
        Cancelled           // Cancelled by customer or admin before or during execution
    }
}
