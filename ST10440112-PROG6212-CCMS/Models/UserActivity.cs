namespace ST10440112_PROG6212_CCMS.Models
{
    public class UserActivity
    {
        public Guid ActivityId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string ActionName { get; set; }
        public DateTime ActivityTime { get; set; }
        public string IPAddress { get; set; }
        public bool Success { get; set; }
        public string Details { get; set; }

        public UserActivity()
        {
            ActivityId = Guid.NewGuid();
            ActivityTime = DateTime.Now;
        }
    }
}
