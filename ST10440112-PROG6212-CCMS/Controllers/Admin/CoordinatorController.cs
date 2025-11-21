using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Controllers.Admin
{
    [Authorize(Roles = "ProgrammeCoordinator")]
    [Route("Admin/Coordinator/[action]")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CoordinatorController> _logger;

        public CoordinatorController(ApplicationDbContext context, ILogger<CoordinatorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Coordinator/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalClaims = await _context.Claims.CountAsync();
                var pendingClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Pending");
                var approvedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Verified" || c.ClaimStatus == "Approved");
                var rejectedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Rejected");
                var totalAmount = await _context.Claims
                    .Where(c => c.ClaimStatus == "Approved")
                    .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

                ViewBag.TotalClaims = totalClaims;
                ViewBag.PendingClaims = pendingClaims;
                ViewBag.ApprovedClaims = approvedClaims;
                ViewBag.RejectedClaims = rejectedClaims;
                ViewBag.TotalAmount = totalAmount;

                var recentClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Include(c => c.Comments)
                    .OrderByDescending(c => c.SubmissionDate)
                    .Take(10)
                    .ToListAsync();

                return View("~/Views/Admin/Dashboard.cshtml", recentClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coordinator dashboard");
                ViewBag.TotalClaims = 0;
                ViewBag.PendingClaims = 0;
                ViewBag.ApprovedClaims = 0;
                ViewBag.RejectedClaims = 0;
                ViewBag.TotalAmount = 0;
                return View("~/Views/Admin/Dashboard.cshtml", new List<Claim>());
            }
        }

        // GET: Admin/Coordinator/Review
        public async Task<IActionResult> Review()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Where(c => c.ClaimStatus == "Pending")
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View("~/Views/Admin/CoordinatorReview.cshtml", pendingClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coordinator review page");
                return View("~/Views/Admin/CoordinatorReview.cshtml", new List<Claim>());
            }
        }

        // POST: Admin/Coordinator/VerifyClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyClaim(Guid claimId, string action, string? comments)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    return NotFound();
                }

                if (action == "verify")
                {
                    claim.ClaimStatus = "Verified";
                    TempData["SuccessMessage"] = "Claim verified successfully and sent to Academic Manager for approval.";
                }
                else if (action == "reject")
                {
                    claim.ClaimStatus = "Rejected";
                    TempData["SuccessMessage"] = "Claim has been rejected.";
                }

                // Add comment if provided
                if (!string.IsNullOrWhiteSpace(comments))
                {
                    var comment = new ClaimComment
                    {
                        CommentId = Guid.NewGuid(),
                        ClaimId = claimId,
                        AuthorName = "Ebrahim Jacobs",
                        AuthorRole = "Programme Coordinator",
                        CommentText = comments,
                        CreatedDate = DateTime.Now
                    };
                    _context.ClaimComments.Add(comment);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Claim {claimId} status updated to {claim.ClaimStatus} by Programme Coordinator");

                return RedirectToAction(nameof(Review));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying claim: {claimId}");
                TempData["ErrorMessage"] = "An error occurred while processing the claim.";
                return RedirectToAction(nameof(Review));
            }
        }

        // GET: Admin/Coordinator/ReviewDetails/5
        [HttpGet("/Admin/Coordinator/ReviewDetails/{id}")]
        public async Task<IActionResult> ReviewDetails(Guid? id)
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

                return View("~/Views/Admin/ReviewDetails.cshtml", claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading review details for claim: {id}");
                TempData["ErrorMessage"] = "Error loading claim details.";
                return RedirectToAction(nameof(Review));
            }
        }

        // GET: Admin/Coordinator/TrackClaims
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                var allClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View("~/Views/Admin/TrackClaims.cshtml", allClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track claims page");
                return View("~/Views/Admin/TrackClaims.cshtml", new List<Claim>());
            }
        }
    }
}
