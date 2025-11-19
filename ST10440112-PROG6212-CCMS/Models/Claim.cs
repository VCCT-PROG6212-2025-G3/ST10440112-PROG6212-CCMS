using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class Claim : IValidatableObject
    {
        [Key]
        public Guid ClaimId { get; set; }

        [Required(ErrorMessage = "Claim status is required")]
        [StringLength(50, ErrorMessage = "Claim status cannot exceed 50 characters")]
        public string ClaimStatus { get; set; } = "Pending";

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Hourly rate must be a positive number")]
        public int HourlyRate { get; set; }

        [Required(ErrorMessage = "Total hours is required")]
        [Range(0.01, 24, ErrorMessage = "Total hours must be between 0.01 and 24 hours per day")]
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
        public virtual ICollection<ClaimComment> Comments { get; set; } = new List<ClaimComment>();

        // Calculated property
        [NotMapped]
        public decimal TotalAmount => (decimal)TotalHours * (decimal)HourlyRate;

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Claim date cannot be in the future
            if (ClaimDate > DateTime.Now)
            {
                results.Add(new ValidationResult("Claim date cannot be in the future", new[] { nameof(ClaimDate) }));
            }

            // Submission date cannot be before claim date
            if (SubmissionDate < ClaimDate)
            {
                results.Add(new ValidationResult("Submission date cannot be before claim date", new[] { nameof(SubmissionDate) }));
            }

            // If approved, approved date should be after submission date
            if (ApprovedDate.HasValue && ApprovedDate < SubmissionDate)
            {
                results.Add(new ValidationResult("Approved date cannot be before submission date", new[] { nameof(ApprovedDate) }));
            }

            // Validate claim status
            var validStatuses = new[] { "Pending", "Verified", "Approved", "Rejected", "Settled" };
            if (!validStatuses.Contains(ClaimStatus))
            {
                results.Add(new ValidationResult($"Claim status must be one of: {string.Join(", ", validStatuses)}", new[] { nameof(ClaimStatus) }));
            }

            return results;
        }
    }
}
