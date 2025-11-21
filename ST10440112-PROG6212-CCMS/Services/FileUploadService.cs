namespace ST10440112_PROG6212_CCMS.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IFileEncryptionService _encryptionService;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".doc", ".xls" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB (increased from 5MB)

        public FileUploadService(
            IWebHostEnvironment environment, 
            ILogger<FileUploadService> logger,
            IFileEncryptionService encryptionService)
        {
            _environment = environment;
            _logger = logger;
            _encryptionService = encryptionService;
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
                    return (false, $"Invalid file type. Only PDF, DOCX, XLSX, DOC, and XLS files are allowed.", null);
                }

                // Validate file size
                if (!IsValidFileSize(file.Length))
                {
                    return (false, $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB.", null);
                }

                // Sanitize filename to prevent path traversal attacks
                var sanitizedFileName = SanitizeFileName(file.FileName);

                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", claimId);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique file name with timestamp
                var fileExtension = GetFileExtension(sanitizedFileName);
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var uniqueFileName = $"{timestamp}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file temporarily (unencrypted)
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"File saved temporarily: {uniqueFileName}");

                // ✅ ENCRYPT THE FILE
                var encryptionResult = await _encryptionService.EncryptFileAsync(filePath);
                if (!encryptionResult.Success)
                {
                    // If encryption fails, delete the unencrypted file
                    DeleteFile(Path.Combine("uploads", claimId, uniqueFileName));
                    return (false, $"File encryption failed: {encryptionResult.Message}", null);
                }

                _logger.LogInformation($"File uploaded and encrypted successfully: {uniqueFileName}");

                // Return relative path for database storage
                var relativePath = Path.Combine("uploads", claimId, uniqueFileName);
                return (true, "File uploaded and encrypted successfully.", relativePath);
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
            return fileSize <= MaxFileSize && fileSize > 0;
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        /// <summary>
        /// Sanitizes filename to prevent security issues
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            // Remove any path information
            fileName = Path.GetFileName(fileName);

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Limit length
            if (sanitized.Length > 200)
            {
                var extension = Path.GetExtension(sanitized);
                sanitized = sanitized.Substring(0, 200 - extension.Length) + extension;
            }

            return sanitized;
        }

        /// <summary>
        /// Gets file info including encryption status
        /// </summary>
        public async Task<(bool Exists, long Size, bool IsEncrypted)> GetFileInfoAsync(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
                if (!File.Exists(fullPath))
                {
                    return (false, 0, false);
                }

                var fileInfo = new FileInfo(fullPath);
                var isEncrypted = await _encryptionService.IsFileEncryptedAsync(fullPath);

                return (true, fileInfo.Length, isEncrypted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file info: {relativePath}");
                return (false, 0, false);
            }
        }
    }
}
