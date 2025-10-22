using System.ComponentModel.DataAnnotations;
using ST10440112_PROG6212_CCMS.Models;
using Xunit;

namespace ST10440112_PROG6212_CCMS.Tests.Models
{
    public class DocumentModelTests
    {
        [Fact]
        public void Document_WithValidData_PassesValidation()
        {
            // Arrange
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/test.pdf",
                UploadDate = DateTime.Now,
                DocType = ".pdf"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(document);
            var isValid = Validator.TryValidateObject(document, context, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Document_Properties_SetCorrectly()
        {
            // Arrange
            var documentId = Guid.NewGuid();
            var claimId = Guid.NewGuid();
            var url = "uploads/claim_docs/document.pdf";
            var uploadDate = DateTime.Now;
            var docType = ".pdf";

            // Act
            var document = new Document
            {
                DocumentID = documentId,
                ClaimId = claimId,
                Url = url,
                UploadDate = uploadDate,
                DocType = docType
            };

            // Assert
            Assert.Equal(documentId, document.DocumentID);
            Assert.Equal(claimId, document.ClaimId);
            Assert.Equal(url, document.Url);
            Assert.Equal(uploadDate, document.UploadDate);
            Assert.Equal(docType, document.DocType);
        }

        [Fact]
        public void Document_SupportsPdfFiles()
        {
            // Arrange & Act
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/report.pdf",
                UploadDate = DateTime.Now,
                DocType = ".pdf"
            };

            // Assert
            Assert.Equal(".pdf", document.DocType);
        }

        [Fact]
        public void Document_SupportsDocxFiles()
        {
            // Arrange & Act
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/report.docx",
                UploadDate = DateTime.Now,
                DocType = ".docx"
            };

            // Assert
            Assert.Equal(".docx", document.DocType);
        }

        [Fact]
        public void Document_SupportsXlsxFiles()
        {
            // Arrange & Act
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/timesheet.xlsx",
                UploadDate = DateTime.Now,
                DocType = ".xlsx"
            };

            // Assert
            Assert.Equal(".xlsx", document.DocType);
        }

        [Fact]
        public void Document_CanBeLinkedToClaim()
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
                IsSettled = false
            };

            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = claimId,
                Url = "uploads/claim_docs/test.pdf",
                UploadDate = DateTime.Now,
                DocType = ".pdf",
                Claim = claim
            };

            // Assert
            Assert.Equal(claimId, document.ClaimId);
            Assert.NotNull(document.Claim);
            Assert.Equal(claim.ClaimId, document.Claim.ClaimId);
        }

        [Fact]
        public void Document_DefaultValues_AreCorrect()
        {
            // Act
            var document = new Document();

            // Assert
            Assert.Equal(Guid.Empty, document.DocumentID);
            Assert.Equal(Guid.Empty, document.ClaimId);
            Assert.Null(document.Url);
            Assert.Equal(default(DateTime), document.UploadDate);
            Assert.Null(document.DocType);
            Assert.Null(document.Claim);
        }

        [Fact]
        public void Document_Url_CanContainPath()
        {
            // Arrange & Act
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/2024/01/document.pdf",
                UploadDate = DateTime.Now,
                DocType = ".pdf"
            };

            // Assert
            Assert.Contains("uploads", document.Url);
            Assert.Contains("claim_docs", document.Url);
            Assert.Contains("2024", document.Url);
        }

        [Fact]
        public void Document_UploadDate_CanBeRetrieved()
        {
            // Arrange
            var uploadDate = new DateTime(2024, 1, 15, 10, 30, 0);
            var document = new Document
            {
                DocumentID = Guid.NewGuid(),
                ClaimId = Guid.NewGuid(),
                Url = "uploads/claim_docs/test.pdf",
                UploadDate = uploadDate,
                DocType = ".pdf"
            };

            // Assert
            Assert.Equal(2024, document.UploadDate.Year);
            Assert.Equal(1, document.UploadDate.Month);
            Assert.Equal(15, document.UploadDate.Day);
        }
    }
}
