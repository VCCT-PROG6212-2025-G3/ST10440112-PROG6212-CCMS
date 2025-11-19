using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using System.Security.Claims;
using ClaimModel = ST10440112_PROG6212_CCMS.Models.Claim;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "AcademicManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ManagerController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard - Overview of claims workflow
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get verified claims awaiting approval
                var verifiedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Verified")
                    .CountAsync();

                // Get approved claims
                var approvedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Approved")
                    .CountAsync();

                // Get rejected claims
                var rejectedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Rejected")
                    .CountAsync();

                // Calculate total value of verified claims
                var totalValue = await _context.Claims
                    .Where(c => c.ClaimStatus == "Verified")
                    .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

                ViewBag.VerifiedClaims = verifiedClaims;
                ViewBag.ApprovedClaims = approvedClaims;
                ViewBag.RejectedClaims = rejectedClaims;
                ViewBag.TotalClaimsValue = totalValue;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        // Review - List verified claims awaiting approval
        public async Task<IActionResult> Review()
        {
            try
            {
                var verifiedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Verified")
                    .Include(c => c.Lecturer)
                    .OrderBy(c => c.SubmissionDate)
                    .ToListAsync();

                return View(verifiedClaims);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading claims: {ex.Message}";
                return View(new List<ClaimModel>());
            }
        }

        // ApproveClaim - Approve a verified claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(Guid id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found";
                    return RedirectToAction("Review");
                }

                if (claim.ClaimStatus != "Verified")
                {
                    TempData["ErrorMessage"] = "Only verified claims can be approved";
                    return RedirectToAction("Review");
                }

                claim.ClaimStatus = "Approved";
                claim.ApprovedDate = DateTime.Now;

                _context.Claims.Update(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Claim for {claim.Lecturer?.Name ?? "Unknown"} has been approved";
                return RedirectToAction("Review");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
                return RedirectToAction("Review");
            }
        }

        // RejectClaim - Reject a verified claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(Guid id, string reason)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found";
                    return RedirectToAction("Review");
                }

                if (claim.ClaimStatus != "Verified")
                {
                    TempData["ErrorMessage"] = "Only verified claims can be rejected";
                    return RedirectToAction("Review");
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    TempData["ErrorMessage"] = "Please provide a reason for rejection";
                    return RedirectToAction("Review");
                }

                claim.ClaimStatus = "Rejected";

                _context.Claims.Update(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Claim for {claim.Lecturer?.Name ?? "Unknown"} has been rejected";
                return RedirectToAction("Review");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
                return RedirectToAction("Review");
            }
        }

        // TrackClaims - View all claims for tracking/auditing
        public async Task<IActionResult> TrackClaims()
        {
            try
            {
                var allClaims = await _context.Claims
                    .Include(c => c.Lecturer)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(allClaims);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading claims: {ex.Message}";
                return View(new List<ClaimModel>());
            }
        }
    }
}
