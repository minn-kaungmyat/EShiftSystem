namespace EShiftSystem.Models.Enums
{
    // defines priority levels for jobs to help with scheduling and resource allocation
    public enum JobPriority
    {
        Normal,     // standard priority job
        High,       // higher priority requiring faster attention
        Urgent      // immediate attention required
    }
}
