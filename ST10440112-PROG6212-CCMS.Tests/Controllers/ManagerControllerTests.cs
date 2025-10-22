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
    public class ManagerControllerTests
    {
        private readonly Mock<ILogger<ManagerController>> _mockLogger;
        private readonly ApplicationDbContext _context;
        private readonly ManagerController _controller;

        public ManagerControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<ManagerController>>();
            _controller = new ManagerController(_context, _mockLogger.Object);

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

            var verifiedClaim = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = lecturer.LecturerId,
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Verified",
                IsSettled = false
            };

            _context.Claims.Add(verifiedClaim);
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
        public async Task Review_ReturnsVerifiedClaims()
        {
            // Act
            var result = await _controller.Review();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Verified", model.First().ClaimStatus);
        }

        [Fact]
        public async Task ApproveClaim_WithApproveAction_UpdatesStatusAndDate()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.ApproveClaim(claim.ClaimId, "approve", "Approved comment");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Review", redirectResult.ActionName);
            
            var updatedClaim = await _context.Claims.FindAsync(claim.ClaimId);
            Assert.Equal("Approved", updatedClaim.ClaimStatus);
            Assert.NotNull(updatedClaim.ApprovedDate);
        }

        [Fact]
        public async Task ApproveClaim_WithRejectAction_UpdatesStatus()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await _controller.ApproveClaim(claim.ClaimId, "reject", "Rejection reason");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Review", redirectResult.ActionName);
            
            var updatedClaim = await _context.Claims.FindAsync(claim.ClaimId);
            Assert.Equal("Rejected", updatedClaim.ClaimStatus);
        }

        [Fact]
        public async Task ApproveClaim_AddsCommentWhenProvided()
        {
            // Arrange
            var claim = await _context.Claims.FirstAsync();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            await _controller.ApproveClaim(claim.ClaimId, "approve", "Manager approval comment");

            // Assert
            var comments = await _context.ClaimComments.Where(c => c.ClaimId == claim.ClaimId).ToListAsync();
            Assert.Single(comments);
            Assert.Equal("Manager approval comment", comments.First().CommentText);
            Assert.Equal("Janet Du Plessis", comments.First().AuthorName);
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
