using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Services;
using System.Security.Claims;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            ApplicationDbContext context,
            IFileEncryptionService encryptionService,
            IWebHostEnvironment environment,
            ILogger<DocumentController> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Download a document securely
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(Guid documentId)
        {
            try
            {
                // Get the document from database
                var document = await _context.Documents
                    .Include(d => d.Claim)
                    .FirstOrDefaultAsync(d => d.DocumentID == documentId);

                if (document == null)
                {
                    _logger.LogWarning($"Document not found: {documentId}");
                    return NotFound();
                }

                // Authorization check - user must be:
                // 1. The lecturer who submitted the claim, OR
                // 2. An admin (ProgrammeCoordinator, AcademicManager, or HR)
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == userEmail);

                var isOwner = lecturer != null && document.Claim.LecturerId == lecturer.LecturerId;
                var isAdmin = User.IsInRole("ProgrammeCoordinator") || User.IsInRole("AcademicManager") || User.IsInRole("HR");

                if (!isOwner && !isAdmin)
                {
                    _logger.LogWarning($"Unauthorized document access attempt: {documentId} by user {userEmail}");
                    return Forbid();
                }

                // Get file path from document URL and construct full path
                var relativePath = document.Url;
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError($"File not found on disk: {filePath}");
                    return NotFound("File not found on the server.");
                }

                // Decrypt the file
                var decryptResult = await _encryptionService.DecryptFileAsync(filePath);
                if (!decryptResult.Success || decryptResult.DecryptedData == null)
                {
                    _logger.LogError($"Failed to decrypt file: {filePath}. Error: {decryptResult.Message}");
                    return StatusCode(500, "Failed to process file.");
                }

                // Get original filename from the document URL
                var fileName = Path.GetFileName(filePath);

                // Return the decrypted file
                return File(decryptResult.DecryptedData, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading document: {documentId}");
                return StatusCode(500, "An error occurred while downloading the document.");
            }
        }

        /// <summary>
        /// Get content type based on file extension
        /// </summary>
        private string GetContentType(string? fileExtension)
        {
            return fileExtension?.ToLower() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "doc" => "application/msword",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "xls" => "application/vnd.ms-excel",
                "txt" => "text/plain",
                "png" => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }
    }
}
