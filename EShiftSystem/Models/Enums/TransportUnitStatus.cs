namespace EShiftSystem.Models.Enums
{
    public enum TransportUnitStatus
    {
        Available,        // Ready to be assigned to a load
        Assigned          // Already assigned to a load (not reusable until released)
    }
} 