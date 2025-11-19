using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class UserSession
    {
        [Key]
        public Guid SessionId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string IPAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LogoutTime { get; set; }

        public UserSession()
        {
            SessionId = Guid.NewGuid();
            LoginTime = DateTime.Now;
            LastActivityTime = DateTime.Now;
            IsActive = true;
        }
    }
}
