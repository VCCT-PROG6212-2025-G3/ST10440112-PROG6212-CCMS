using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalClaims = await _context.Claims.CountAsync();
                var pendingClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Pending" || c.ClaimStatus == "Verified");
                var approvedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Approved");
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
                    .OrderByDescending(c => c.SubmissionDate)
                    .Take(10)
                    .ToListAsync();

                return View(recentClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                ViewBag.TotalClaims = 0;
                ViewBag.PendingClaims = 0;
                ViewBag.ApprovedClaims = 0;
                ViewBag.RejectedClaims = 0;
                ViewBag.TotalAmount = 0;
                return View(new List<Claim>());
            }
        }

        // GET: Admin/CoordinatorReview
        public async Task<IActionResult> CoordinatorReview()
        {
            try
            {
                // Get all pending claims that need coordinator review
                var pendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Where(c => c.ClaimStatus == "Pending")
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coordinator review page");
                return View(new List<Claim>());
            }
        }

        // GET: Admin/ReviewClaim/5
        public async Task<IActionResult> ReviewClaim(Guid? id)
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
                _logger.LogError(ex, $"Error loading claim for review: {id}");
                return RedirectToAction(nameof(CoordinatorReview));
            }
        }

        // POST: Admin/VerifyClaim
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

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Claim {claimId} status updated to {claim.ClaimStatus} by Coordinator");

                return RedirectToAction(nameof(CoordinatorReview));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying claim: {claimId}");
                TempData["ErrorMessage"] = "An error occurred while processing the claim.";
                return RedirectToAction(nameof(CoordinatorReview));
            }
        }
        // GET: Admin/ManagerReview
        public async Task<IActionResult> ManagerReview()
        {
            try
            {
                // Get all verified claims that need manager approval
                var verifiedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Where(c => c.ClaimStatus == "Verified")
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(verifiedClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager review page");
                return View(new List<Claim>());
            }
        }

        // POST: Admin/ApproveClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(Guid claimId, string action, string? comments)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    return NotFound();
                }

                if (action == "approve")
                {
                    claim.ClaimStatus = "Approved";
                    claim.ApprovedDate = DateTime.Now;
                    TempData["SuccessMessage"] = "Claim has been approved successfully.";
                }
                else if (action == "reject")
                {
                    claim.ClaimStatus = "Rejected";
                    TempData["SuccessMessage"] = "Claim has been rejected.";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Claim {claimId} status updated to {claim.ClaimStatus} by Academic Manager");

                return RedirectToAction(nameof(ManagerReview));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving/rejecting claim: {claimId}");
                TempData["ErrorMessage"] = "An error occurred while processing the claim.";
                return RedirectToAction(nameof(ManagerReview));
            }
        }
        // GET: Admin/TrackClaims
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                var allClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(allClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading track claims page");
                return View(new List<Claim>());
            }
        }
    }
}
