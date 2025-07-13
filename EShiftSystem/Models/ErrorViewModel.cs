namespace EShiftSystem.Models
{
    // view model for displaying error information to users
    public class ErrorViewModel
    {
        // unique request identifier for error tracking
        public string? RequestId { get; set; }

        // determines whether to show the request id in error views
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
