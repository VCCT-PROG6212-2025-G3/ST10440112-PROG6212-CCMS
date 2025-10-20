using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class Document
    {
        [Key]
        public Guid DocumentID { get; set; }

        [Required(ErrorMessage = "Document URL/Path is required")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string Url { get; set; } = string.Empty;

        [Required(ErrorMessage = "Upload date is required")]
        [DataType(DataType.Date)]
        public DateTime UploadDate { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
        public string DocType { get; set; } = string.Empty;

        // Foreign Key
        [Required]
        public Guid ClaimId { get; set; }

        // Navigation property
        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
    }
}
