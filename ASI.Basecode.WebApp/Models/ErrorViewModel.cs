namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// View model used by the error page to display request-level diagnostic information.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// The unique identifier for the current HTTP request. Useful for tracing errors.
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether a request id is available and should be shown in the UI.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}








