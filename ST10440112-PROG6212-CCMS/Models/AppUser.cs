using Microsoft.AspNetCore.Identity;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; } // "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR"
        public Guid? LecturerId { get; set; } // Foreign key to Lecturer (if user is a lecturer)
        public Guid? AdminId { get; set; } // Foreign key to Admin (if user is coordinator/manager)
    }
}
