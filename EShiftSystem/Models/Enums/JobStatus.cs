namespace EShiftSystem.Models.Enums
{
    // defines the various states a job can be in throughout its lifecycle
    public enum JobStatus
    {
        Pending,            // job booked by customer, awaiting admin review
        Approved,           // admin has approved the job
        InProgress,         // at least one load is being handled (e.g., driver assigned, on the way)
        Delivered,          // all loads have been delivered by transport units
        Completed,          // all loads confirmed as received/completed by customer
        Cancelled           // cancelled by customer or admin before or during execution
    }
}
