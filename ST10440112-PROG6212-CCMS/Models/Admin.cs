using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Models
{
    public abstract class Admin
    {
        [Key]
        public Guid AdminID { get; set; }

        [Required(ErrorMessage = "Admin name is required")]
        [StringLength(100, ErrorMessage = "Admin name cannot exceed 100 characters")]
        public string AdminName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string AdminEmail { get; set; } = string.Empty;
    }
}
