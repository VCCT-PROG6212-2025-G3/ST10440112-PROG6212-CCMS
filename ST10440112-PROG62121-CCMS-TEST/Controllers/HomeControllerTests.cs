using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ST10440112_PROG6212_CCMS.Controllers;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using Xunit;

namespace ST10440112_PROG62121_CCMS_TEST.Contolllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly ApplicationDbContext _context;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object, _context);

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

            var claim1 = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = lecturer.LecturerId,
                HourlyRate = 350,
                TotalHours = 40,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Approved",
                IsSettled = false
            };

            var claim2 = new Claim
            {
                ClaimId = Guid.NewGuid(),
                LecturerId = lecturer.LecturerId,
                HourlyRate = 350,
                TotalHours = 30,
                ClaimDate = DateTime.Now,
                SubmissionDate = DateTime.Now,
                ClaimStatus = "Pending",
                IsSettled = false
            };

            _context.Claims.AddRange(claim1, claim2);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsViewWithStatistics()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["TotalClaims"]);
            Assert.NotNull(viewResult.ViewData["ApprovedClaims"]);
            Assert.NotNull(viewResult.ViewData["PendingClaims"]);
            Assert.NotNull(viewResult.ViewData["TotalAmount"]);
        }

        [Fact]
        public async Task Index_CalculatesStatisticsCorrectly()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(2, viewResult.ViewData["TotalClaims"]);
            Assert.Equal(1, viewResult.ViewData["ApprovedClaims"]);
            Assert.Equal(1, viewResult.ViewData["PendingClaims"]);
        }

        [Fact]
        public async Task Index_ReturnsRecentClaims()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
