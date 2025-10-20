using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class Claim
    {
        [Key]
        public Guid ClaimId { get; set; }

        [Required(ErrorMessage = "Claim status is required")]
        [StringLength(50, ErrorMessage = "Claim status cannot exceed 50 characters")]
        public string ClaimStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Hourly rate must be a positive number")]
        public int HourlyRate { get; set; }

        [Required(ErrorMessage = "Total hours is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total hours must be a positive number")]
        public float TotalHours { get; set; }

        [Required(ErrorMessage = "Claim date is required")]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; }

        [Required(ErrorMessage = "Submission date is required")]
        [DataType(DataType.Date)]
        public DateTime SubmissionDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ApprovedDate { get; set; }

        public bool IsSettled { get; set; } = false;

        // Foreign Key
        [Required]
        public Guid LecturerId { get; set; }

        // Navigation properties
        [ForeignKey("LecturerId")]
        public virtual Lecturer? Lecturer { get; set; }

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
