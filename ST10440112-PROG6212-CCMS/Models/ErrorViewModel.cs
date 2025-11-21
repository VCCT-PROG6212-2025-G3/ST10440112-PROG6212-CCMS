namespace ST10440112_PROG6212_CCMS.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string Message { get; set; } = "An error occurred";
        public bool ShowDetails { get; set; }
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
