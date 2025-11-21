using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    /// <summary>
    /// Global error handler to prevent death loops and provide user-friendly error pages
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles application errors and prevents redirect loops
        /// </summary>
        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            // Log the error
            if (exception != null)
            {
                _logger.LogError(exception, "Unhandled exception occurred at path: {Path}", 
                    exceptionHandlerPathFeature?.Path ?? "Unknown");
            }

            // Check if this is a redirect loop (too many redirects)
            var referer = Request.Headers["Referer"].ToString();
            var currentPath = Request.Path.ToString();
            
            // Prevent infinite redirect loops
            if (referer.Contains("/Error") || HttpContext.Items.ContainsKey("ErrorHandled"))
            {
                _logger.LogWarning("Potential redirect loop detected. Showing fallback error page.");
                return View("FallbackError");
            }

            HttpContext.Items["ErrorHandled"] = true;

            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = GetUserFriendlyMessage(exception),
                ShowDetails = IsDevelopment(),
                ExceptionType = exception?.GetType().Name,
                StackTrace = IsDevelopment() ? exception?.StackTrace : null
            };

            return View(errorViewModel);
        }

        /// <summary>
        /// Handles 404 Not Found errors
        /// </summary>
        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            Response.StatusCode = 404;
            _logger.LogWarning("404 Not Found: {Path}", Request.Path);
            
            return View("NotFound");
        }

        /// <summary>
        /// Handles 403 Forbidden errors
        /// </summary>
        [Route("Error/403")]
        public IActionResult Forbidden403()
        {
            Response.StatusCode = 403;
            _logger.LogWarning("403 Forbidden: {Path} by user {User}", 
                Request.Path, User.Identity?.Name ?? "Anonymous");
            
            return View("Forbidden");
        }

        /// <summary>
        /// Handles 500 Internal Server errors
        /// </summary>
        [Route("Error/500")]
        public IActionResult InternalServerError500()
        {
            Response.StatusCode = 500;
            _logger.LogError("500 Internal Server Error: {Path}", Request.Path);
            
            return View("InternalServerError");
        }

        /// <summary>
        /// Converts technical exceptions to user-friendly messages
        /// </summary>
        private string GetUserFriendlyMessage(Exception? exception)
        {
            if (exception == null)
                return "An unexpected error occurred. Please try again.";

            return exception switch
            {
                UnauthorizedAccessException => "You don't have permission to access this resource.",
                FileNotFoundException => "The requested file could not be found.",
                InvalidOperationException => "The operation could not be completed. Please try again.",
                TimeoutException => "The request took too long to complete. Please try again.",
                _ => "An unexpected error occurred. Our team has been notified."
            };
        }

        /// <summary>
        /// Test route to manually trigger an exception for testing error handling
        /// </summary>
        [Route("Error/Test")]
        public IActionResult TestError()
        {
            throw new Exception("This is a test exception to demonstrate error handling and the custom error page.");
        }

        /// <summary>
        /// Checks if running in development environment
        /// </summary>
        private bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment == "Development";
        }
    }

    /// <summary>
    /// Error view model
    /// </summary>
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
