using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;
using ST10440112_PROG6212_CCMS.ViewModels;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(
            ApplicationDbContext context,
            IFileUploadService fileUploadService,
            ILogger<ClaimsController> logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            var model = new ClaimSubmissionViewModel();
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

                // Get the first lecturer from database (In a real app, this would come from authentication)
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync();
                if (lecturer == null)
                {
                    ModelState.AddModelError("", "No lecturer found in the system. Please ensure seed data is loaded.");
                    return View(model);
                }

                // Create new claim
                var claim = new Claim
                {
                    ClaimId = Guid.NewGuid(),
                    LecturerId = lecturer.LecturerId,
                    HourlyRate = model.HourlyRate,
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
                // Get the first lecturer (In a real app, this would come from authentication)
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync();
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
    }
}
