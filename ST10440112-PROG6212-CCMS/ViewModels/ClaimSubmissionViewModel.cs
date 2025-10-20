using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.ViewModels
{
    public class ClaimSubmissionViewModel
    {
        [Required(ErrorMessage = "Teaching hours is required")]
        [Range(0, 999, ErrorMessage = "Teaching hours must be between 0 and 999")]
        [Display(Name = "Teaching Time (Hrs)")]
        public int TeachingHours { get; set; }

        [Required(ErrorMessage = "Lecture preparation hours is required")]
        [Range(0, 999, ErrorMessage = "Lecture prep hours must be between 0 and 999")]
        [Display(Name = "Lecture Prep Time (Hrs)")]
        public int LecturePrepHours { get; set; }

        [Required(ErrorMessage = "Admin and marking hours is required")]
        [Range(0, 999, ErrorMessage = "Admin hours must be between 0 and 999")]
        [Display(Name = "Admin Tasks Time (Hrs)")]
        public int AdminHours { get; set; }

        [Display(Name = "Total Time (Hrs)")]
        public int TotalHours => TeachingHours + LecturePrepHours + AdminHours;

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, 10000, ErrorMessage = "Hourly rate must be between 1 and 10000")]
        [Display(Name = "Hourly Rate (R)")]
        public int HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => TotalHours * HourlyRate;

        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? AdditionalNotes { get; set; }

        [Display(Name = "Supporting Documents")]
        public List<IFormFile>? Documents { get; set; }

        // For displaying uploaded file names
        public List<string> UploadedFileNames { get; set; } = new List<string>();
    }
}
