namespace ST10440112_PROG6212_CCMS.Services
{
    /// <summary>
    /// Helper service to route users to their appropriate dashboards based on their role after login.
    /// </summary>
    public interface ILoginRedirectHelper
    {
        /// <summary>
        /// Determines the appropriate dashboard URL based on user role.
        /// </summary>
        /// <param name="userRole">The user's role (e.g., "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR")</param>
        /// <returns>The controller and action name for the user's dashboard</returns>
        (string Controller, string Action) GetDashboardRoute(string? userRole);
    }

    public class LoginRedirectHelper : ILoginRedirectHelper
    {
        /// <summary>
        /// Maps user roles to their appropriate dashboard routes.
        /// </summary>
        private static readonly Dictionary<string, (string Controller, string Action)> RoleDashboardMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Lecturer", ("Lecturer", "Dashboard") },
                { "ProgrammeCoordinator", ("Coordinator", "Dashboard") },
                { "AcademicManager", ("Manager", "Dashboard") },
                { "HR", ("HR", "Dashboard") }
            };

        public (string Controller, string Action) GetDashboardRoute(string? userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole))
            {
                // Default fallback if no role is set
                return ("Home", "Index");
            }

            if (RoleDashboardMap.TryGetValue(userRole, out var route))
            {
                return route;
            }

            // Default fallback for unknown roles
            return ("Home", "Index");
        }
    }
}
