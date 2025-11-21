using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using System.Security.Claims;
using ClaimModel = ST10440112_PROG6212_CCMS.Models.Claim;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "ProgrammeCoordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CoordinatorController> _logger;

        // Verification rules (configurable constants)
        private const float MaxHoursPerClaim = 160f;
        private const float MinHoursPerClaim = 0.5f;
        private const int MaxHourlyRate = 10000;
        private const int MinHourlyRate = 100;

        public CoordinatorController(ApplicationDbContext context, ILogger<CoordinatorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Coordinator/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Pending")
                    .CountAsync();

                var verifiedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Verified")
                    .CountAsync();

                var rejectedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Rejected")
                    .CountAsync();

                var totalClaimsValue = await _context.Claims
                    .Where(c => c.ClaimStatus == "Verified" || c.ClaimStatus == "Approved")
                    .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

                ViewBag.PendingClaims = pendingClaims;
                ViewBag.VerifiedClaims = verifiedClaims;
                ViewBag.RejectedClaims = rejectedClaims;
                ViewBag.TotalClaimsValue = totalClaimsValue;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading coordinator dashboard");
                return View();
            }
        }

        // GET: Coordinator/VerifyClaims
        public async Task<IActionResult> VerifyClaims()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Pending")
                    .OrderBy(c => c.SubmissionDate)
                    .ToListAsync();

                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending claims");
                return View(new List<ClaimModel>());
            }
        }

        // POST: Coordinator/AutoVerifyAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoVerifyAll()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Pending")
                    .ToListAsync();

                int verifiedCount = 0;
                int rejectedCount = 0;
                var verificationResults = new List<string>();

                foreach (var claim in pendingClaims)
                {
                    var verificationResult = VerifyClaimAutomatically(claim);

                    if (verificationResult.IsValid)
                    {
                        claim.ClaimStatus = "Verified";
                        verifiedCount++;
                        verificationResults.Add($"✓ Claim {claim.ClaimId} verified");
                    }
                    else
                    {
                        claim.ClaimStatus = "Rejected";
                        rejectedCount++;
                        verificationResults.Add($"✗ Claim {claim.ClaimId} rejected: {verificationResult.Reason}");
                    }
                }

                if (pendingClaims.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Auto-verification complete: {verifiedCount} verified, {rejectedCount} rejected";
                return RedirectToAction(nameof(VerifyClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during auto-verification");
                TempData["ErrorMessage"] = "An error occurred during verification";
                return RedirectToAction(nameof(VerifyClaims));
            }
        }

        // POST: Coordinator/VerifyClaim/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyClaim(Guid id)
        {
            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Lecturer)
                    .FirstOrDefaultAsync(c => c.ClaimId == id);

                if (claim == null)
                {
                    return NotFound();
                }

                var verificationResult = VerifyClaimAutomatically(claim);

                if (verificationResult.IsValid)
                {
                    claim.ClaimStatus = "Verified";
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Claim verified successfully!";
                }
                else
                {
                    claim.ClaimStatus = "Rejected";
                    await _context.SaveChangesAsync();
                    TempData["ErrorMessage"] = $"Claim rejected: {verificationResult.Reason}";
                }

                return RedirectToAction(nameof(VerifyClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying claim");
                TempData["ErrorMessage"] = "An error occurred while verifying the claim";
                return RedirectToAction(nameof(VerifyClaims));
            }
        }

        // POST: Coordinator/RejectClaim/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(Guid id, string reason = "")
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);

                if (claim == null)
                {
                    return NotFound();
                }

                claim.ClaimStatus = "Rejected";
                await _context.SaveChangesAsync();

                // Add comment about rejection
                if (!string.IsNullOrEmpty(reason))
                {
                    var comment = new ClaimComment
                    {
                        CommentId = Guid.NewGuid(),
                        ClaimId = claim.ClaimId,
                        AuthorName = User.FindFirst("FullName")?.Value ?? "Coordinator",
                        AuthorRole = "ProgrammeCoordinator",
                        CommentText = $"Claim rejected: {reason}",
                        CreatedDate = DateTime.Now
                    };
                    _context.ClaimComments.Add(comment);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Claim rejected successfully";
                return RedirectToAction(nameof(VerifyClaims));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim");
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim";
                return RedirectToAction(nameof(VerifyClaims));
            }
        }

        // GET: Coordinator/VerifiedClaims
        public async Task<IActionResult> VerifiedClaims()
        {
            try
            {
                var verifiedClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Verified")
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(verifiedClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading verified claims");
                return View(new List<ClaimModel>());
            }
        }

        // Automatic verification logic based on predefined rules
        private VerificationResult VerifyClaimAutomatically(ClaimModel claim)
        {
            // Rule 1: Check if hours are within acceptable range
            if (claim.TotalHours < MinHoursPerClaim || claim.TotalHours > MaxHoursPerClaim)
            {
                return new VerificationResult
                {
                    IsValid = false,
                    Reason = $"Hours ({claim.TotalHours}) outside acceptable range ({MinHoursPerClaim} - {MaxHoursPerClaim})"
                };
            }

            // Rule 2: Check if hourly rate is within acceptable range
            if (claim.HourlyRate < MinHourlyRate || claim.HourlyRate > MaxHourlyRate)
            {
                return new VerificationResult
                {
                    IsValid = false,
                    Reason = $"Hourly rate (R {claim.HourlyRate}) outside acceptable range (R {MinHourlyRate} - R {MaxHourlyRate})"
                };
            }

            // Rule 3: Check if lecturer exists
            if (claim.Lecturer == null)
            {
                return new VerificationResult
                {
                    IsValid = false,
                    Reason = "Lecturer not found in system"
                };
            }

            // Rule 4: Check if claim date is not in the future
            if (claim.ClaimDate > DateTime.Now)
            {
                return new VerificationResult
                {
                    IsValid = false,
                    Reason = "Claim date cannot be in the future"
                };
            }

            // Rule 5: Check if submission date is reasonable (within 30 days of claim date)
            var daysDifference = (claim.SubmissionDate - claim.ClaimDate).TotalDays;
            if (daysDifference > 30)
            {
                return new VerificationResult
                {
                    IsValid = false,
                    Reason = $"Claim submitted {daysDifference:F0} days after claim date (max 30 days)"
                };
            }

            // All checks passed
            return new VerificationResult
            {
                IsValid = true,
                Reason = "Claim verified successfully"
            };
        }

        // Helper class for verification results
        private class VerificationResult
        {
            public bool IsValid { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
