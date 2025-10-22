using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ST10440112_PROG6212_CCMS.Controllers;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;
using ST10440112_PROG6212_CCMS.ViewModels;
using Microsoft.EntityFrameworkCore.InMemory; // Add this using directive to resolve the 'UseInMemoryDatabase' method.
using Xunit;

namespace ST10440112_PROG62121_CCMS_TEST.Contolllers
{
    public class ClaimsControllerTests
    {
        private readonly Mock<IFileUploadService> _mockFileUploadService;
        private readonly Mock<ILogger<ClaimsController>> _mockLogger;
        private readonly ApplicationDbContext _context;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            // Setup InMemory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockFileUploadService = new Mock<IFileUploadService>();
            _mockLogger = new Mock<ILogger<ClaimsController>>();

            _controller = new ClaimsController(_context, _mockFileUploadService.Object, _mockLogger.Object);

            // Seed test data
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
            _context.SaveChanges();
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ClaimSubmissionViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Create_POST_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new ClaimSubmissionViewModel
            {
                HourlyRate = 350,
                TeachingHours = 20,
                LecturePrepHours = 10,
                AdminHours = 10
            };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Single(_context.Claims);
        }

        [Fact]
        public async Task Index_ReturnsViewWithClaims()
        {
            // Arrange
            var lecturer = await _context.Lecturers.FirstAsync();
            var claim = new Claim
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
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var lecturer = await _context.Lecturers.FirstAsync();
            var claim = new Claim
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
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(claim.ClaimId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Claim>(viewResult.Model);
            Assert.Equal(claim.ClaimId, model.ClaimId);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddDocuments_WithPendingClaim_ReturnsViewResult()
        {
            // Arrange
            var lecturer = await _context.Lecturers.FirstAsync();
            var claim = new Claim
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
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddDocuments(claim.ClaimId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Claim>(viewResult.Model);
            Assert.Equal(claim.ClaimId, model.ClaimId);
        }

        [Fact]
        public async Task AddDocuments_POST_WithNoDocuments_RedirectsWithError()
        {
            // Arrange
            var lecturer = await _context.Lecturers.FirstAsync();
            var claim = new Claim
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
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.AddDocuments(claim.ClaimId, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AddDocuments", redirectResult.ActionName);
        }
    }
}
