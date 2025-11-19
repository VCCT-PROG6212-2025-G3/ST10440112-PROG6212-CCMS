using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Services
{
    public class SessionManagementService : ISessionManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SessionManagementService> _logger;

        public SessionManagementService(ApplicationDbContext context, ILogger<SessionManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserSession> CreateSessionAsync(string userId, string email, string userRole, string ipAddress)
        {
            try
            {
                var userSession = new UserSession
                {
                    UserId = userId,
                    Email = email,
                    UserRole = userRole,
                    IPAddress = ipAddress,
                    IsActive = true
                };

                _context.UserSessions.Add(userSession);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Session created for user {email} with role {userRole}");
                return userSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating session for user {email}");
                throw;
            }
        }

        public async Task<UserSession> GetSessionAsync(Guid sessionId)
        {
            return await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<UserSession> GetActiveSessionByUserAsync(string userId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LoginTime)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserSession>> GetAllActiveSessionsAsync()
        {
            return await _context.UserSessions
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.LoginTime)
                .ToListAsync();
        }

        public async Task UpdateLastActivityAsync(Guid sessionId)
        {
            try
            {
                var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session != null)
                {
                    session.LastActivityTime = DateTime.Now;
                    _context.UserSessions.Update(session);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating last activity for session {sessionId}");
            }
        }

        public async Task EndSessionAsync(Guid sessionId)
        {
            try
            {
                var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session != null)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.Now;
                    _context.UserSessions.Update(session);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Session {sessionId} ended for user {session.Email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error ending session {sessionId}");
            }
        }

        public async Task<bool> IsSessionActiveAsync(Guid sessionId)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            return session?.IsActive ?? false;
        }

        public async Task LogActivityAsync(string userId, string email, string userRole, string action,
            string controller, string actionName, string ipAddress, bool success, string details = null)
        {
            try
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    Email = email,
                    UserRole = userRole,
                    Action = action,
                    Controller = controller,
                    ActionName = actionName,
                    IPAddress = ipAddress,
                    Success = success,
                    Details = details
                };

                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Activity logged: {action} by {email} in {controller}/{actionName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging activity for user {email}");
            }
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int days = 7)
        {
            var sinceDate = DateTime.Now.AddDays(-days);
            return await _context.UserActivities
                .Where(a => a.UserId == userId && a.ActivityTime >= sinceDate)
                .OrderByDescending(a => a.ActivityTime)
                .ToListAsync();
        }

        public async Task<List<UserActivity>> GetAllActivitiesAsync(int days = 7)
        {
            var sinceDate = DateTime.Now.AddDays(-days);
            return await _context.UserActivities
                .Where(a => a.ActivityTime >= sinceDate)
                .OrderByDescending(a => a.ActivityTime)
                .ToListAsync();
        }
    }
}
