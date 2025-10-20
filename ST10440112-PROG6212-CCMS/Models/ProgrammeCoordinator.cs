using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class ProgrammeCoordinator : Admin
    {
        [Required(ErrorMessage = "Major/Degree is required")]
        [StringLength(100, ErrorMessage = "Major/Degree cannot exceed 100 characters")]
        public string MajorDegree { get; set; } = string.Empty;
    }
}
