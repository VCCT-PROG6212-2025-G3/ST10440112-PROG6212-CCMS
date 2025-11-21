using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using System.Text;

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

        // GET: Admin/GenerateReport
        public async Task<IActionResult> GenerateReport(string reportType = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var claims = _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Approved");

                // Filter by date range if provided
                if (startDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate <= endDate.Value);
                }

                var claimList = await claims.OrderByDescending(c => c.ApprovedDate).ToListAsync();

                ViewBag.ReportType = reportType;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.TotalClaims = claimList.Count;
                ViewBag.TotalAmount = claimList.Sum(c => (decimal)c.TotalHours * c.HourlyRate);
                ViewBag.TotalHours = claimList.Sum(c => c.TotalHours);

                return View(claimList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["ErrorMessage"] = "Error generating report";
                return View(new List<Claim>());
            }
        }

        // GET: Admin/ExportReportCSV
        public async Task<IActionResult> ExportReportCSV(string reportType = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var claims = _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Approved");

                if (startDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate <= endDate.Value);
                }

                var claimList = await claims.OrderByDescending(c => c.ApprovedDate).ToListAsync();

                // Generate CSV
                var csv = new StringBuilder();
                csv.AppendLine("Admin Report - Newlands University Claims Management System");
                csv.AppendLine($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");
                csv.AppendLine();
                csv.AppendLine("Lecturer Name,Email,Claim Date,Hours,Hourly Rate,Total Amount,Approved Date,Status");

                foreach (var claim in claimList)
                {
                    var totalAmount = (decimal)claim.TotalHours * claim.HourlyRate;
                    var lecturerName = claim.Lecturer?.Name ?? "Unknown";
                    var lecturerEmail = claim.Lecturer?.Email ?? "";
                    csv.AppendLine($"{lecturerName},{lecturerEmail},{claim.ClaimDate:dd/MM/yyyy},{claim.TotalHours:F1},R {claim.HourlyRate:N2},R {totalAmount:N2},{claim.ApprovedDate:dd/MM/yyyy},{claim.ClaimStatus}");
                }

                csv.AppendLine();
                csv.AppendLine($"Total Claims,{claimList.Count}");
                csv.AppendLine($"Total Hours,{claimList.Sum(c => c.TotalHours):F1}");
                csv.AppendLine($"Total Amount,R {claimList.Sum(c => (decimal)c.TotalHours * c.HourlyRate):N2}");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"Admin_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["ErrorMessage"] = "Error exporting report";
                return RedirectToAction(nameof(GenerateReport));
            }
        }

        // GET: Admin/ExportReportPDF
        public async Task<IActionResult> ExportReportPDF(string reportType = "all", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var claims = _context.Claims
                    .Include(c => c.Lecturer)
                    .Where(c => c.ClaimStatus == "Approved");

                if (startDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    claims = claims.Where(c => c.ClaimDate <= endDate.Value);
                }

                var claimList = await claims.OrderByDescending(c => c.ApprovedDate).ToListAsync();

                // Generate HTML content for PDF export
                var htmlContent = GenerateReportPDF(claimList);
                var bytes = Encoding.UTF8.GetBytes(htmlContent);

                var fileName = $"Admin_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                return File(bytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report as PDF");
                TempData["ErrorMessage"] = "Error exporting report";
                return RedirectToAction(nameof(GenerateReport));
            }
        }

        // Helper method to generate report in PDF HTML format
        private string GenerateReportPDF(List<Claim> claims)
        {
            var totalAmount = claims.Sum(c => (decimal)c.TotalHours * c.HourlyRate);
            var totalHours = claims.Sum(c => c.TotalHours);

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset=\"utf-8\">");
            html.AppendLine("<title>Admin Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #13325B; text-align: center; }");
            html.AppendLine("h2 { color: #13325B; margin-top: 20px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            html.AppendLine("th { background-color: #13325B; color: white; padding: 8px; text-align: left; }");
            html.AppendLine("td { border: 1px solid #ddd; padding: 8px; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine(".summary { margin-top: 20px; padding: 10px; background-color: #f0f0f0; border-left: 4px solid #13325B; }");
            html.AppendLine(".summary p { margin: 5px 0; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine("<h1>Newlands University - Admin Report</h1>");
            html.AppendLine($"<p style=\"text-align: center; color: #666;\">Generated: {DateTime.Now:dd MMM yyyy HH:mm}</p>");

            html.AppendLine("<h2>Report Summary</h2>");
            html.AppendLine("<div class=\"summary\">");
            html.AppendLine($"<p><strong>Total Claims:</strong> {claims.Count}</p>");
            html.AppendLine($"<p><strong>Total Hours:</strong> {totalHours:F1}</p>");
            html.AppendLine($"<p><strong>Total Amount:</strong> R {totalAmount:N2}</p>");
            html.AppendLine($"<p><strong>Average per Claim:</strong> R {(claims.Count > 0 ? (totalAmount / claims.Count) : 0):N2}</p>");
            html.AppendLine("</div>");

            html.AppendLine("<h2>Claim Details</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>Lecturer</th>");
            html.AppendLine("<th>Email</th>");
            html.AppendLine("<th>Claim Date</th>");
            html.AppendLine("<th>Hours</th>");
            html.AppendLine("<th>Hourly Rate</th>");
            html.AppendLine("<th>Total Amount</th>");
            html.AppendLine("<th>Approved Date</th>");
            html.AppendLine("<th>Payment Status</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var claim in claims)
            {
                var claimTotal = (decimal)claim.TotalHours * claim.HourlyRate;
                var paymentStatus = claim.IsSettled ? "Settled" : "Pending";
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{(claim.Lecturer?.Name ?? "Unknown")}</td>");
                html.AppendLine($"<td>{claim.Lecturer?.Email}</td>");
                html.AppendLine($"<td>{claim.ClaimDate:dd MMM yyyy}</td>");
                html.AppendLine($"<td>{claim.TotalHours:F1}</td>");
                html.AppendLine($"<td>R {claim.HourlyRate:N2}</td>");
                html.AppendLine($"<td>R {claimTotal:N2}</td>");
                html.AppendLine($"<td>{claim.ApprovedDate:dd MMM yyyy}</td>");
                html.AppendLine($"<td>{paymentStatus}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            html.AppendLine("<div class=\"summary\" style=\"margin-top: 20px;\">");
            html.AppendLine($"<p><strong>Total Claims:</strong> {claims.Count}</p>");
            html.AppendLine($"<p><strong>Total Hours:</strong> {totalHours:F1}</p>");
            html.AppendLine($"<p><strong>Total Amount:</strong> R {totalAmount:N2}</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}
