using System.ComponentModel.DataAnnotations;
using ST10440112_PROG6212_CCMS.Models;
using Xunit;

namespace ST10440112_PROG6212_CCMS.Tests.Models
{
    public class ClaimCommentModelTests
    {
        [Fact]
        public void ClaimComment_WithValidData_PassesValidation()
        {
            // Arrange
            var comment = new ClaimComment
            {
                CommentId = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                AuthorName = "Ebrahim Jacobs",
                AuthorRole = "Programme Coordinator",
                CommentText = "This claim looks good and has been verified.",
                CreatedDate = DateTime.Now
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(comment);
            var isValid = Validator.TryValidateObject(comment, context, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void ClaimComment_Properties_SetCorrectly()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var claimId = Guid.NewGuid();
            var authorName = "Janet Du Plessis";
            var authorRole = "Academic Manager";
            var commentText = "Approved for payment";
            var createdDate = DateTime.Now;

            // Act
            var comment = new ClaimComment
            {
                CommentId = commentId,
                ClaimId = claimId,
                AuthorName = authorName,
                AuthorRole = authorRole,
                CommentText = commentText,
                CreatedDate = createdDate
            };

            // Assert
            Assert.Equal(commentId, comment.CommentId);
            Assert.Equal(claimId, comment.ClaimId);
            Assert.Equal(authorName, comment.AuthorName);
            Assert.Equal(authorRole, comment.AuthorRole);
            Assert.Equal(commentText, comment.CommentText);
            Assert.Equal(createdDate, comment.CreatedDate);
        }

        [Fact]
        public void ClaimComment_MultipleComments_CanBeAddedToClaim()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var claim = new Claim
            {
                ClaimId = claimId,
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false,
                Comments = new List<ClaimComment>()
            };

            var comment1 = new ClaimComment
            {
                CommentId = Guid.NewGuid(),
                ClaimId = claimId,
                AuthorName = "Ebrahim Jacobs",
                AuthorRole = "Programme Coordinator",
                CommentText = "First comment",
                CreatedDate = DateTime.Now
            };

            var comment2 = new ClaimComment
            {
                CommentId = Guid.NewGuid(),
                ClaimId = claimId,
                AuthorName = "Janet Du Plessis",
                AuthorRole = "Academic Manager",
                CommentText = "Second comment",
                CreatedDate = DateTime.Now.AddMinutes(10)
            };

            // Act
            claim.Comments.Add(comment1);
            claim.Comments.Add(comment2);

            // Assert
            Assert.Equal(2, claim.Comments.Count);
            Assert.Contains(comment1, claim.Comments);
            Assert.Contains(comment2, claim.Comments);
        }
    }
}
