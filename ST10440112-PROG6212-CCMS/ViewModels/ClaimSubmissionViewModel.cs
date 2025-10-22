using System.ComponentModel.DataAnnotations;
using ST10440112_PROG6212_CCMS.Attributes;

namespace ST10440112_PROG6212_CCMS.ViewModels
{
    public class ClaimSubmissionViewModel
    {
        [Required(ErrorMessage = "Hourly rate is required.")]
        [ValidHourlyRate(100, 1000)]
        [Display(Name = "Hourly Rate (R)")]
        public int HourlyRate { get; set; }

        [Required(ErrorMessage = "Total hours worked is required.")]
        [ValidHours(0.5f, 200f)]
        [Display(Name = "Total Hours Worked")]
        public float TotalHours { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount => (decimal)(TotalHours * HourlyRate);

        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
        [DataType(DataType.MultilineText)]
        public string? AdditionalNotes { get; set; }

        [Display(Name = "Supporting Documents")]
        [ValidFile]
        public List<IFormFile>? Documents { get; set; }

        // For displaying uploaded file names
        public List<string> UploadedFileNames { get; set; } = new List<string>();
    }
}
