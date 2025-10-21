using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Controllers.Admin
{
    [Route("Admin/Manager/[action]")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ApplicationDbContext context, ILogger<ManagerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Manager/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalClaims = await _context.Claims.CountAsync();
                var pendingClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Verified");
                var approvedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Approved");
                var rejectedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Rejected");
                var totalAmount = await _context.Claims
                    .Where(c => c.ClaimStatus == "Approved")
                    .SumAsync(c => c.TotalHours * c.HourlyRate);

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
                _logger.LogError(ex, "Error loading manager dashboard");
                ViewBag.TotalClaims = 0;
                ViewBag.PendingClaims = 0;
                ViewBag.ApprovedClaims = 0;
                ViewBag.RejectedClaims = 0;
                ViewBag.TotalAmount = 0;
                return View("~/Views/Admin/Dashboard.cshtml", new List<Claim>());
            }
        }

        // GET: Admin/Manager/Review
        public async Task<IActionResult> Review()
        {
            try
            {
                var verifiedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Documents)
                    .Where(c => c.ClaimStatus == "Verified")
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View("~/Views/Admin/ManagerReview.cshtml", verifiedClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager review page");
                return View("~/Views/Admin/ManagerReview.cshtml", new List<Claim>());
            }
        }

        // POST: Admin/Manager/ApproveClaim
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

                // Add comment if provided
                if (!string.IsNullOrWhiteSpace(comments))
                {
                    var comment = new ClaimComment
                    {
                        CommentId = Guid.NewGuid(),
                        ClaimId = claimId,
                        AuthorName = "Janet Du Plessis",
                        AuthorRole = "Academic Manager",
                        CommentText = comments,
                        CreatedDate = DateTime.Now
                    };
                    _context.ClaimComments.Add(comment);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Claim {claimId} status updated to {claim.ClaimStatus} by Academic Manager");

                return RedirectToAction(nameof(Review));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving/rejecting claim: {claimId}");
                TempData["ErrorMessage"] = "An error occurred while processing the claim.";
                return RedirectToAction(nameof(Review));
            }
        }

        // GET: Admin/Manager/ReviewDetails/5
        [HttpGet("/Admin/Manager/ReviewDetails/{id}")]
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
                return RedirectToAction(nameof(Review));
            }
        }

        // GET: Admin/Manager/TrackClaims
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
