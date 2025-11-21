using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Services;
using Microsoft.EntityFrameworkCore;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize]
    public class SecureFileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SecureFileController> _logger;

        public SecureFileController(
            ApplicationDbContext context,
            IFileEncryptionService encryptionService,
            IWebHostEnvironment environment,
            ILogger<SecureFileController> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Downloads and decrypts a document
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(Guid documentId)
        {
            try
            {
                // Get document from database
                var document = await _context.Documents
                    .Include(d => d.Claim)
                    .FirstOrDefaultAsync(d => d.DocumentID == documentId);

                if (document == null)
                {
                    return NotFound("Document not found.");
                }

                // Check authorization - user must be associated with the claim
                var userEmail = User.Identity?.Name;
                var isAuthorized = await IsUserAuthorizedForDocument(document.ClaimId, userEmail);

                if (!isAuthorized)
                {
                    _logger.LogWarning($"Unauthorized access attempt to document {documentId} by {userEmail}");
                    return Forbid();
                }

                // Get full file path
                var fullPath = Path.Combine(_environment.WebRootPath, document.Url);

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("File not found on server.");
                }

                // Decrypt the file
                var decryptionResult = await _encryptionService.DecryptFileAsync(fullPath);

                if (!decryptionResult.Success || decryptionResult.DecryptedData == null)
                {
                    _logger.LogError($"Failed to decrypt file: {document.Url}");
                    return StatusCode(500, "Error decrypting file.");
                }

                // Determine content type based on file extension
                var contentType = GetContentType(document.Url);

                // Get original filename from URL
                var fileName = Path.GetFileName(document.Url);

                // Return decrypted file
                _logger.LogInformation($"File {documentId} downloaded and decrypted by {userEmail}");
                return File(decryptionResult.DecryptedData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading document {documentId}");
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }

        /// <summary>
        /// Views a document inline (decrypted)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> View(Guid documentId)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.Claim)
                    .FirstOrDefaultAsync(d => d.DocumentID == documentId);

                if (document == null)
                {
                    return NotFound("Document not found.");
                }

                var userEmail = User.Identity?.Name;
                var isAuthorized = await IsUserAuthorizedForDocument(document.ClaimId, userEmail);

                if (!isAuthorized)
                {
                    return Forbid();
                }

                var fullPath = Path.Combine(_environment.WebRootPath, document.Url);

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound("File not found on server.");
                }

                // Decrypt the file
                var decryptionResult = await _encryptionService.DecryptFileAsync(fullPath);

                if (!decryptionResult.Success || decryptionResult.DecryptedData == null)
                {
                    return StatusCode(500, "Error decrypting file.");
                }

                var contentType = GetContentType(document.Url);

                // Return for inline viewing
                return File(decryptionResult.DecryptedData, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing document {documentId}");
                return StatusCode(500, "An error occurred while viewing the file.");
            }
        }

        /// <summary>
        /// Checks if user is authorized to access a document
        /// </summary>
        private async Task<bool> IsUserAuthorizedForDocument(Guid claimId, string? userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
                return false;

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
                return false;

            // Check if user is the lecturer who owns the claim
            if (claim.Lecturer?.Email == userEmail)
                return true;

            // Check if user is HR, Coordinator, or Manager (they can access all documents)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null && (user.Role == "HR" || user.Role == "ProgrammeCoordinator" || user.Role == "AcademicManager"))
                return true;

            return false;
        }

        /// <summary>
        /// Gets content type based on file extension
        /// </summary>
        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}
