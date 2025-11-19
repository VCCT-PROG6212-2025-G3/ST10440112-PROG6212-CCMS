using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Services
{
    public interface ISessionManagementService
    {
        // Session Management
        Task<UserSession> CreateSessionAsync(string userId, string email, string userRole, string ipAddress);
        Task<UserSession> GetSessionAsync(Guid sessionId);
        Task<UserSession> GetActiveSessionByUserAsync(string userId);
        Task<List<UserSession>> GetAllActiveSessionsAsync();
        Task UpdateLastActivityAsync(Guid sessionId);
        Task EndSessionAsync(Guid sessionId);
        Task<bool> IsSessionActiveAsync(Guid sessionId);

        // Activity Logging
        Task LogActivityAsync(string userId, string email, string userRole, string action,
            string controller, string actionName, string ipAddress, bool success, string details = null);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int days = 7);
        Task<List<UserActivity>> GetAllActivitiesAsync(int days = 7);
    }
}
