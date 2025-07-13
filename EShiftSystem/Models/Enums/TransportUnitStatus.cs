namespace EShiftSystem.Models.Enums
{
    // defines operational status of transport units for resource management
    public enum TransportUnitStatus
    {
        Available,        // ready to be assigned to a load
        Assigned          // already assigned to a load (not reusable until released)
    }
} 