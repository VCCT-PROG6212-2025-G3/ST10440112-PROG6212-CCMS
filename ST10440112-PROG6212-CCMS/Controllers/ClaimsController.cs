using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;
using ST10440112_PROG6212_CCMS.ViewModels;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(
            ApplicationDbContext context,
            IFileUploadService fileUploadService,
            UserManager<AppUser> userManager,
            ILogger<ClaimsController> logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Claims/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Email == user.Email);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer profile not found. Please contact HR.";
                return RedirectToAction("Index", "Home");
            }

            var model = new ClaimSubmissionViewModel
            {
                HourlyRate = lecturer.HourlyRate
            };
            
            ViewBag.LecturerName = lecturer.Name;
            return View(model);
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimSubmissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get the current user and their lecturer profile
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Email == user.Email);
                if (lecturer == null)
                {
                    ModelState.AddModelError("", "Lecturer profile not found.");
                    return View(model);
                }

                // Validation: Max hours per month
                // Check total hours claimed in current month
                var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var currentMonthClaims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId && c.ClaimDate >= currentMonthStart)
                    .SumAsync(c => c.TotalHours);

                if (currentMonthClaims + model.TotalHours > 180)
                {
                    ModelState.AddModelError("TotalHours", $"Total hours for this month cannot exceed 180. You have already claimed {currentMonthClaims} hours.");
                    model.HourlyRate = lecturer.HourlyRate; // Ensure rate is preserved
                    return View(model);
                }

                // Create new claim
                var claim = new Claim
                {
                    ClaimId = Guid.NewGuid(),
                    LecturerId = lecturer.LecturerId,
                    HourlyRate = lecturer.HourlyRate, // Use rate from profile, not model input
                    TotalHours = model.TotalHours,
                    ClaimDate = DateTime.Now,
                    SubmissionDate = DateTime.Now,
                    ClaimStatus = "Pending",
                    IsSettled = false
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Handle file uploads
                if (model.Documents != null && model.Documents.Any())
                {
                    foreach (var file in model.Documents)
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
                            ModelState.AddModelError("Documents", uploadResult.Message);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating claim");
                ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");
                return View(model);
            }
        }

        // GET: Claims
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get the current user and their lecturer profile
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Email == user.Email);
                if (lecturer == null)
                {
                    return View(new List<Claim>());
                }

                var claims = await _context.Claims
                    .Where(c => c.LecturerId == lecturer.LecturerId)
                    .Include(c => c.Documents)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims");
                return View(new List<Claim>());
            }
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Include(c => c.Comments)
                    .FirstOrDefaultAsync(m => m.ClaimId == id);

                if (claim == null)
                {
                    return NotFound();
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving claim details for ID: {id}");
                return RedirectToAction(nameof(Index));
            }
        }
        // GET: Claims/AddDocuments/5
        public async Task<IActionResult> AddDocuments(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .FirstOrDefaultAsync(m => m.ClaimId == id);

                if (claim == null)
                {
                    return NotFound();
                }

                // Only allow adding documents if claim is still pending
                if (claim.ClaimStatus != "Pending")
                {
                    TempData["ErrorMessage"] = "Cannot add documents to a claim that has already been reviewed.";
                    return RedirectToAction(nameof(Details), new { id = claim.ClaimId });
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading add documents page for claim: {id}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Claims/AddDocuments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocuments(Guid claimId, List<IFormFile> documents)
        {
            try
            {
                if (documents == null || !documents.Any())
                {
                    TempData["ErrorMessage"] = "Please select at least one document to upload.";
                    return RedirectToAction(nameof(AddDocuments), new { id = claimId });
                }

                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    return NotFound();
                }

                // Only allow adding documents if claim is still pending
                if (claim.ClaimStatus != "Pending")
                {
                    TempData["ErrorMessage"] = "Cannot add documents to a claim that has already been reviewed.";
                    return RedirectToAction(nameof(Details), new { id = claimId });
                }

                int uploadedCount = 0;
                foreach (var file in documents)
                {
                    var uploadResult = await _fileUploadService.UploadFileAsync(file, claimId.ToString());

                    if (uploadResult.Success)
                    {
                        var document = new Document
                        {
                            DocumentID = Guid.NewGuid(),
                            ClaimId = claimId,
                            Url = uploadResult.FilePath!,
                            UploadDate = DateTime.Now,
                            DocType = _fileUploadService.GetFileExtension(file.FileName)
                        };

                        _context.Documents.Add(document);
                        uploadedCount++;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to upload file: {uploadResult.Message}");
                        TempData["ErrorMessage"] = uploadResult.Message;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{uploadedCount} document(s) uploaded successfully!";
                return RedirectToAction(nameof(Details), new { id = claimId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding documents to claim");
                TempData["ErrorMessage"] = "An error occurred while uploading documents. Please try again.";
                return RedirectToAction(nameof(AddDocuments), new { id = claimId });
            }
        }
    }
}
