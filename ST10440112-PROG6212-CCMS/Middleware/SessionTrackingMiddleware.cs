using ST10440112_PROG6212_CCMS.Services;
using System.Security.Claims;

namespace ST10440112_PROG6212_CCMS.Middleware
{
    public class SessionTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionTrackingMiddleware> _logger;

        public SessionTrackingMiddleware(RequestDelegate next, ILogger<SessionTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISessionManagementService sessionManagementService)
        {
            try
            {
                if (context.User.Identity?.IsAuthenticated ?? false)
                {
                    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var email = context.User.FindFirst(ClaimTypes.Email)?.Value;
                    var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

                    if (!string.IsNullOrEmpty(userId))
                    {
                        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                        // Update last activity time for existing session
                        var session = await sessionManagementService.GetActiveSessionByUserAsync(userId);
                        if (session != null)
                        {
                            await sessionManagementService.UpdateLastActivityAsync(session.SessionId);
                        }

                        // Store session info in HttpContext for view access
                        context.Items["UserSession"] = session;
                        context.Items["UserIP"] = ipAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionTrackingMiddleware");
            }

            await _next(context);
        }
    }
}
