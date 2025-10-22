using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using ST10440112_PROG6212_CCMS.Services;
using Xunit;

namespace ST10440112_PROG6212_CCMS.Tests.Services
{
    public class FileUploadServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly FileUploadService _service;
        private readonly string _testRootPath;

        public FileUploadServiceTests()
        {
            _testRootPath = Path.Combine(Path.GetTempPath(), "TestUploads");
            Directory.CreateDirectory(_testRootPath);

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testRootPath);

            _service = new FileUploadService(_mockEnvironment.Object);
        }

        [Fact]
        public void GetFileExtension_ReturnsCorrectExtension()
        {
            // Arrange
            var fileName = "document.pdf";

            // Act
            var result = _service.GetFileExtension(fileName);

            // Assert
            Assert.Equal(".pdf", result);
        }

        [Fact]
        public void GetFileExtension_HandlesNoExtension()
        {
            // Arrange
            var fileName = "document";

            // Act
            var result = _service.GetFileExtension(fileName);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public async Task UploadFileAsync_WithValidPdfFile_ReturnsSuccess()
        {
            // Arrange
            var content = "Test PDF content";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream));

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FilePath);
            Assert.Contains(claimId, result.FilePath);

            // Cleanup
            if (File.Exists(Path.Combine(_testRootPath, result.FilePath)))
            {
                File.Delete(Path.Combine(_testRootPath, result.FilePath));
            }
        }

        [Fact]
        public async Task UploadFileAsync_WithValidDocxFile_ReturnsSuccess()
        {
            // Arrange
            var content = "Test DOCX content";
            var fileName = "test.docx";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream));

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FilePath);

            // Cleanup
            if (File.Exists(Path.Combine(_testRootPath, result.FilePath)))
            {
                File.Delete(Path.Combine(_testRootPath, result.FilePath));
            }
        }

        [Fact]
        public async Task UploadFileAsync_WithValidXlsxFile_ReturnsSuccess()
        {
            // Arrange
            var content = "Test XLSX content";
            var fileName = "test.xlsx";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream));

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.FilePath);

            // Cleanup
            if (File.Exists(Path.Combine(_testRootPath, result.FilePath)))
            {
                File.Delete(Path.Combine(_testRootPath, result.FilePath));
            }
        }

        [Fact]
        public async Task UploadFileAsync_WithInvalidFileType_ReturnsFailure()
        {
            // Arrange
            var fileName = "test.exe";
            var ms = new MemoryStream();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(1024);

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Only PDF, DOCX, and XLSX files are allowed", result.Message);
        }

        [Fact]
        public async Task UploadFileAsync_WithOversizedFile_ReturnsFailure()
        {
            // Arrange
            var fileName = "test.pdf";
            var ms = new MemoryStream();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11MB

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("File size must not exceed 10MB", result.Message);
        }

        [Fact]
        public async Task UploadFileAsync_WithEmptyFile_ReturnsFailure()
        {
            // Arrange
            var fileName = "test.pdf";
            var ms = new MemoryStream();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(0);

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await _service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("File is empty", result.Message);
        }

        [Fact]
        public async Task UploadFileAsync_CreatesDirectoryIfNotExists()
        {
            // Arrange
            var testPath = Path.Combine(Path.GetTempPath(), "NewTestDir");
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(testPath);
            var service = new FileUploadService(mockEnv.Object);

            var content = "Test content";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream));

            var claimId = Guid.NewGuid().ToString();

            // Act
            var result = await service.UploadFileAsync(fileMock.Object, claimId);

            // Assert
            Assert.True(result.Success);
            Assert.True(Directory.Exists(testPath));

            // Cleanup
            if (Directory.Exists(testPath))
            {
                Directory.Delete(testPath, true);
            }
        }
    }
}
