using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10440112_PROG6212_CCMS.Models
{
    public class ClaimComment
    {
        [Key]
        public Guid CommentId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [StringLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AuthorRole { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string CommentText { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        [ForeignKey("ClaimId")]
        public virtual Claim? Claim { get; set; }
    }
}
