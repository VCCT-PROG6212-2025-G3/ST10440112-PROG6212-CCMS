using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class Lecturer
    {
        [Key]
        public Guid LecturerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(100, 10000, ErrorMessage = "Hourly rate must be between 100 and 10000")]
        public decimal HourlyRate { get; set; }

        // Navigation property
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
