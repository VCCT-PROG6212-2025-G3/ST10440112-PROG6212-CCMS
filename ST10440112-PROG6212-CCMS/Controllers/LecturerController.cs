using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;
using System.Security.Claims;
using ClaimModel = ST10440112_PROG6212_CCMS.Models.Claim;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LecturerController> _logger;
        private readonly IFileUploadService _fileUploadService;

        public LecturerController(ApplicationDbContext context, UserManager<AppUser> userManager, ILogger<LecturerController> logger, IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        // GET: Lecturer/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var lecturerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    return Unauthorized();
                }

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == lecturerEmail);

                if (lecturer == null)
                {
                    _logger.LogWarning($"Dashboard: No lecturer found for email {lecturerEmail}");
                    return NotFound("Lecturer profile not found");
                }

                _logger.LogInformation($"Dashboard: Lecturer ID = {lecturer.LecturerId}, Email = {lecturer.Email}");

                var totalClaims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId)
                    .CountAsync();

                var approvedClaims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId && c.ClaimStatus == "Approved")
                    .CountAsync();

                var pendingClaims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId && c.ClaimStatus == "Pending")
                    .CountAsync();

                var totalEarnings = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId && (c.ClaimStatus == "Approved" || c.IsSettled))
                    .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

                ViewBag.Lecturer = lecturer;
                ViewBag.TotalClaims = totalClaims;
                ViewBag.ApprovedClaims = approvedClaims;
                ViewBag.PendingClaims = pendingClaims;
                ViewBag.TotalEarnings = totalEarnings;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lecturer dashboard");
                return View();
            }
        }

        // GET: Lecturer/SubmitClaim
        public async Task<IActionResult> SubmitClaim()
        {
            try
            {
                var lecturerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    return Unauthorized();
                }

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == lecturerEmail);

                if (lecturer == null)
                {
                    return NotFound("Lecturer profile not found");
                }

                // Create a new claim with lecturer's hourly rate pre-filled
                var claim = new ClaimModel
                {
                    LecturerId = lecturer.LecturerId,
                    HourlyRate = lecturer.HourlyRate,
                    ClaimDate = DateTime.Now,
                    SubmissionDate = DateTime.Now
                };

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claim submission form");
                return View();
            }
        }

        // POST: Lecturer/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimModel claim, List<IFormFile>? documents)
        {
            try
            {
                var lecturerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    return Unauthorized();
                }

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == lecturerEmail);

                if (lecturer == null)
                {
                    return NotFound("Lecturer profile not found");
                }

                // Ensure the claim belongs to the authenticated lecturer
                claim.LecturerId = lecturer.LecturerId;
                // Force hourly rate from lecturer record (prevent tampering)
                claim.HourlyRate = lecturer.HourlyRate;
                claim.ClaimStatus = "Pending";
                claim.SubmissionDate = DateTime.Now;

                // Validate total hours
                if (claim.TotalHours <= 0 || claim.TotalHours > 160)
                {
                    ModelState.AddModelError("TotalHours", "Total hours must be between 0 and 160");
                }

                if (ModelState.IsValid)
                {
                    _context.Add(claim);
                    await _context.SaveChangesAsync();

                    // Handle file uploads
                    if (documents != null && documents.Any())
                    {
                        foreach (var file in documents)
                        {
                            if (file.Length > 0)
                            {
                                var uploadResult = await _fileUploadService.UploadFileAsync(file, claim.ClaimId.ToString());

                                if (uploadResult.Success)
                                {
                                    var document = new Document
                                    {
                                        DocumentID = Guid.NewGuid(),
                                        ClaimId = claim.ClaimId,
                                        Url = uploadResult.FilePath!,
                                        UploadDate = DateTime.Now,
                                        DocType = _fileUploadService.GetFileExtension(file.FileName)
                                    };

                                    _context.Documents.Add(document);
                                }
                                else
                                {
                                    _logger.LogWarning($"Failed to upload file: {uploadResult.Message}");
                                    TempData["WarningMessage"] = $"Claim submitted but file upload failed: {uploadResult.Message}";
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = $"Claim submitted successfully! You will earn R {((decimal)claim.TotalHours * claim.HourlyRate):N2}";
                    return RedirectToAction(nameof(MyClaims));
                }

                // Re-populate hourly rate for display
                claim.HourlyRate = (int)lecturer.HourlyRate;
                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                ModelState.AddModelError(string.Empty, "An error occurred while submitting your claim.");
                return View(claim);
            }
        }

        // GET: Lecturer/MyClaims
        public async Task<IActionResult> MyClaims()
        {
            try
            {
                var lecturerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    return Unauthorized();
                }

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == lecturerEmail);

                if (lecturer == null)
                {
                    _logger.LogWarning($"MyClaims: No lecturer found for email {lecturerEmail}");
                    return NotFound("Lecturer profile not found");
                }

                _logger.LogInformation($"MyClaims: Searching for claims with LecturerId = {lecturer.LecturerId}");

                var claims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                _logger.LogInformation($"MyClaims: Found {claims.Count} claims for lecturer {lecturer.Name}");

                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claims: {ErrorMessage}", ex.Message);
                TempData["ErrorMessage"] = $"Error loading claims: {ex.Message}";
                return View(new List<ClaimModel>());
            }
        }

        // GET: Lecturer/ClaimDetail/{id}
        public async Task<IActionResult> ClaimDetail(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var lecturerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(lecturerEmail))
                {
                    return Unauthorized();
                }

                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == lecturerEmail);

                if (lecturer == null)
                {
                    return NotFound("Lecturer profile not found");
                }

                var claim = await _context.Claims
                    .Include(c => c.Documents)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(c => c.ClaimId == id && c.LecturerId == lecturer.LecturerId);

                if (claim == null)
                {
                    return NotFound();
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading claim detail");
                return NotFound();
            }
        }
    }
}
