using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ST10440112_PROG6212_CCMS.Filters
{
    /// <summary>
    /// Custom filter to enforce role-based access control and prevent unauthorized page access.
    /// Maps roles to allowed controller routes.
    /// </summary>
    public class RoleBasedAccessFilter : IAuthorizationFilter
    {
        private readonly ILogger<RoleBasedAccessFilter> _logger;

        public RoleBasedAccessFilter(ILogger<RoleBasedAccessFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // If user is not authenticated, let the [Authorize] attribute handle it
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            var controller = context.RouteData.Values["controller"]?.ToString()?.ToLower();
            var action = context.RouteData.Values["action"]?.ToString()?.ToLower();

            // Exempt Account controller from role-based access checks (handles login, logout, AccessDenied)
            if (controller == "account")
            {
                return;
            }

            var userRole = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Define role-to-controller mappings
            var allowedRoutes = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Lecturer", new List<string> { "home", "lecturer", "claim" } },
                { "ProgrammeCoordinator", new List<string> { "home", "coordinator", "claim", "lecturer" } },
                { "AcademicManager", new List<string> { "home", "manager", "claim", "lecturer" } },
                { "HR", new List<string> { "home", "hr", "lecturer" } }
            };

            // Check if user's role has access to this controller
            if (!string.IsNullOrEmpty(userRole) && allowedRoutes.ContainsKey(userRole))
            {
                var allowedControllers = allowedRoutes[userRole];

                if (!allowedControllers.Contains(controller))
                {
                    _logger.LogWarning($"Access denied: User with role '{userRole}' tried to access '{controller}' controller");
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
