using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class AcademicManager : Admin
    {
        [Required(ErrorMessage = "Faculty is required")]
        [StringLength(100, ErrorMessage = "Faculty cannot exceed 100 characters")]
        public string Faculty { get; set; } = string.Empty;
    }
}
