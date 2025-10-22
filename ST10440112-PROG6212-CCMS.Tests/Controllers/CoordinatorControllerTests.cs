using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ST10440112_PROG6212_CCMS.Controllers.Admin;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using Xunit;

namespace ST10440112_PROG6212_CCMS.Tests.Controllers
{
    public class CoordinatorControllerTests
    {
        private readonly Mock<ILogger<CoordinatorController>> _mockLogger;
        private readonly ApplicationDbContext _context;
        private readonly CoordinatorController _controller;

        public CoordinatorControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<CoordinatorController>>();
            _controller = new CoordinatorController(_context, _mockLogger.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            var lecturer = new Lecturer
            {
                LecturerId = Guid.NewGuid(),
                Name = "Test Lecturer",
                Email = "test@newlands.ac.za",
                Department = "Computer Science"
            };

            _context.Lecturers.Add(lecturer);

            var pendingClaim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = lecturer.LecturerId,
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            _context.Claims.Add(pendingClaim);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Dashboard_ReturnsViewWithStatistics()
        {
            // Act
            var result = await _controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Admin/Dashboard.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async Task Review_ReturnsPendingClaims()
        {
            // Act
            var result = await _controller.Review();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Pending", model.First().ClaimStatus);
        }

        [Fact]
        public async Task VerifyClaim_WithVerifyAction_UpdatesStatus()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.VerifyClaim(claim.ClaimId, "verify", "Test comment");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Review", redirectResult.ActionName);
            
            var updatedClaim = await _context.Claims.FindAsync(claim.ClaimId);
            Assert.Equal("Verified", updatedClaim.ClaimStatus);
        }

        [Fact]
        public async Task VerifyClaim_WithRejectAction_UpdatesStatus()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.VerifyClaim(claim.ClaimId, "reject", "Rejection reason");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Review", redirectResult.ActionName);
            
            var updatedClaim = await _context.Claims.FindAsync(claim.ClaimId);
            Assert.Equal("Rejected", updatedClaim.ClaimStatus);
        }

        [Fact]
        public async Task VerifyClaim_AddsCommentWhenProvided()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            await _controller.VerifyClaim(claim.ClaimId, "verify", "Test comment");

            // Assert
            var comments = await _context.ClaimComments.Where(c => c.ClaimId == claim.ClaimId).ToListAsync();
            Assert.Single(comments);
            Assert.Equal("Test comment", comments.First().CommentText);
            Assert.Equal("Ebrahim Jacobs", comments.First().AuthorName);
        }

        [Fact]
        public async Task ReviewDetails_WithValidId_ReturnsView()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();

            // Act
            var result = await _controller.ReviewDetails(claim.ClaimId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Admin/ReviewDetails.cshtml", viewResult.ViewName);
        }

        [Fact]
        public async Task ReviewDetails_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.ReviewDetails(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
