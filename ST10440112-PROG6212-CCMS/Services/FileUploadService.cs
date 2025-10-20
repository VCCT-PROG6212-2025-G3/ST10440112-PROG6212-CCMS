namespace ST10440112_PROG6212_CCMS.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile file, string claimId)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return (false, "No file selected or file is empty.", null);
                }

                // Validate file type
                if (!IsValidFileType(file.FileName))
                {
                    return (false, $"Invalid file type. Only PDF, DOCX, and XLSX files are allowed.", null);
                }

                // Validate file size
                if (!IsValidFileSize(file.Length))
                {
                    return (false, $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB.", null);
                }

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", claimId);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique file name
                var fileExtension = GetFileExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"File uploaded successfully: {uniqueFileName}");

                // Return relative path for database storage
                var relativePath = Path.Combine("uploads", claimId, uniqueFileName);
                return (true, "File uploaded successfully.", relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"File deleted successfully: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return false;
            }
        }

        public bool IsValidFileType(string fileName)
        {
            var extension = GetFileExtension(fileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(long fileSize)
        {
            return fileSize <= MaxFileSize;
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }
    }
}
