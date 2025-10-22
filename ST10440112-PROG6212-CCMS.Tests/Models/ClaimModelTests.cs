using System.ComponentModel.DataAnnotations;
using ST10440112_PROG6212_CCMS.Models;
using Xunit;

namespace ST10440112_PROG6212_CCMS.Tests.Models
{
    public class ClaimModelTests
    {
        [Fact]
        public void Claim_WithValidData_PassesValidation()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(claim);
            var isValid = Validator.TryValidateObject(claim, context, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Claim_Properties_SetCorrectly()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var lecturerId = Guid.NewGuid();
            var hourlyRate = 350;
            var totalHours = 40.5f;
            var claimDate = DateTime.Now;
            var submissionDate = DateTime.Now;
            var status = "Approved";

            // Act
            var claim = new Claim
            {
                ClaimId = claimId,
                LecturerId = lecturerId,
                HourlyRate = hourlyRate,
                TotalHours = totalHours,
                ClaimDate = claimDate,
                SubmissionDate = submissionDate,
                ClaimStatus = status,
                IsSettled = true,
                ApprovedDate = DateTime.Now
            };

            // Assert
            Assert.Equal(claimId, claim.ClaimId);
            Assert.Equal(lecturerId, claim.LecturerId);
            Assert.Equal(hourlyRate, claim.HourlyRate);
            Assert.Equal(totalHours, claim.TotalHours);
            Assert.Equal(claimDate, claim.ClaimDate);
            Assert.Equal(submissionDate, claim.SubmissionDate);
            Assert.Equal(status, claim.ClaimStatus);
            Assert.True(claim.IsSettled);
            Assert.NotNull(claim.ApprovedDate);
        }

        [Fact]
        public void Claim_CanHaveDocuments()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                Url = "uploads/test.pdf",
                UploadDate = DateTime.Now,
                DocType = ".pdf"
            };

            // Act
            claim.Documents = new List<Document> { document };

            // Assert
            Assert.NotNull(claim.Documents);
            Assert.Single(claim.Documents);
            Assert.Equal(document.ClaimId, claim.ClaimId);
        }

        [Fact]
        public void Claim_CanHaveComments()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            var comment = new ClaimComment
            {
                CommentId = Guid.NewGuid(),
                ClaimId = claim.ClaimId,
                AuthorName = "Test Coordinator",
                AuthorRole = "Programme Coordinator",
                CommentText = "Test comment",
                CreatedDate = DateTime.Now
            };

            // Act
            claim.Comments = new List<ClaimComment> { comment };

            // Assert
            Assert.NotNull(claim.Comments);
            Assert.Single(claim.Comments);
            Assert.Equal(comment.ClaimId, claim.ClaimId);
        }

        [Fact]
        public void Claim_DefaultValues_AreCorrect()
        {
            // Act
            var claim = new Claim();

            // Assert
            Assert.Equal(Guid.Empty, claim.ClaimId);
            Assert.Equal(Guid.Empty, claim.LecturerId);
            Assert.Equal(0, claim.HourlyRate);
            Assert.Equal(0, claim.TotalHours);
            Assert.False(claim.IsSettled);
            Assert.Null(claim.ApprovedDate);
            Assert.Null(claim.Lecturer);
            Assert.Null(claim.Documents);
            Assert.Null(claim.Comments);
        }

        [Fact]
        public void Claim_CalculatesTotalAmount_Correctly()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            // Act
            var totalAmount = claim.TotalHours * claim.HourlyRate;

            // Assert
            Assert.Equal(14000, totalAmount);
        }

        [Fact]
        public void Claim_StatusTransition_Pending_To_Verified()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            // Act
            claim.ClaimStatus = "Verified";

            // Assert
            Assert.Equal("Verified", claim.ClaimStatus);
        }

        [Fact]
        public void Claim_StatusTransition_Verified_To_Approved()
        {
            // Arrange
            var claim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = Guid.NewGuid(),
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Verified",
                IsSettled = false
            };

            // Act
            claim.ClaimStatus = "Approved";
            claim.ApprovedDate = DateTime.Now;

            // Assert
            Assert.Equal("Approved", claim.ClaimStatus);
            Assert.NotNull(claim.ApprovedDate);
        }
    }
}
