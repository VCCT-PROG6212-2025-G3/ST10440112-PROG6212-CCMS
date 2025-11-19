using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;
using System.Text;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HRController> _logger;

        public HRController(ApplicationDbContext context, ILogger<HRController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: HR/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalLecturers = await _context.Lecturers.CountAsync();
                var totalApprovedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Approved");
                var totalPaymentAmount = await _context.Claims
                    .Where(c => c.ClaimStatus == "Approved" && !c.IsSettled)
                    .SumAsync(c => c.TotalHours * c.HourlyRate);

                ViewBag.TotalLecturers = totalLecturers;
                ViewBag.TotalApprovedClaims = totalApprovedClaims;
                ViewBag.TotalPaymentAmount = totalPaymentAmount;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading HR dashboard");
                return View();
            }
        }

        // GET: HR/ManageLecturers
        public async Task<IActionResult> ManageLecturers(string searchTerm = "")
        {
            try
            {
                IQueryable<Lecturer> query = _context.Lecturers;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(l => l.Name.Contains(searchTerm) || l.Email.Contains(searchTerm));
                }

                var lecturers = await query.OrderBy(l => l.Name).ToListAsync();
                return View(lecturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lecturers");
                return View(new List<Lecturer>());
            }
        }

        // GET: HR/EditLecturer/{id}
        public async Task<IActionResult> EditLecturer(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // POST: HR/EditLecturer/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(Guid id, Lecturer lecturer)
        {
            if (id != lecturer.LecturerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lecturer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Lecturer '{lecturer.Name}' updated successfully!";
                    return RedirectToAction(nameof(ManageLecturers));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LecturerExists(lecturer.LecturerId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating lecturer");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the lecturer.");
                }
            }

            return View(lecturer);
        }

        // GET: HR/DeleteLecturer/{id}
        public async Task<IActionResult> DeleteLecturer(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(m => m.LecturerId == id);
            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // POST: HR/DeleteLecturer/{id}
        [HttpPost, ActionName("DeleteLecturer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLecturerConfirmed(Guid id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer != null)
            {
                try
                {
                    _context.Lecturers.Remove(lecturer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Lecturer '{lecturer.Name}' deleted successfully!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting lecturer");
                    TempData["ErrorMessage"] = "An error occurred while deleting the lecturer.";
                }
            }

            return RedirectToAction(nameof(ManageLecturers));
        }

        // GET: HR/ApprovedClaims
        public async Task<IActionResult> ApprovedClaims()
        {
            try
            {
                var approvedClaims = await _context.Claims
                    .Where(c => c.ClaimStatus == "Approved" && !c.IsSettled)
                    .Include(c => c.Lecturer)
                    .OrderByDescending(c => c.ApprovedDate)
                    .ToListAsync();

                return View(approvedClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading approved claims");
                return View(new List<Claim>());
            }
        }

        // POST: HR/MarkAsSettled/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSettled(Guid id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                try
                {
                    claim.IsSettled = true;
                    _context.Update(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Claim marked as settled!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error marking claim as settled");
                    TempData["ErrorMessage"] = "An error occurred while updating the claim.";
                }
            }

            return RedirectToAction(nameof(ApprovedClaims));
        }

        // GET: HR/GenerateInvoice/{id}
        public async Task<IActionResult> GenerateInvoice(Guid id)
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

                if (claim.ClaimStatus != "Approved")
                {
                    TempData["ErrorMessage"] = "Only approved claims can generate invoices";
                    return RedirectToAction(nameof(ApprovedClaims));
                }

                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice");
                TempData["ErrorMessage"] = "Error generating invoice";
                return RedirectToAction(nameof(ApprovedClaims));
            }
        }

        // GET: HR/ExportInvoicePDF/{id}
        public async Task<IActionResult> ExportInvoicePDF(Guid id)
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

                // Generate CSV content for PDF representation
                var csvContent = GenerateInvoiceCSV(claim);
                var bytes = Encoding.UTF8.GetBytes(csvContent);

                var fileName = $"Invoice_{claim.Lecturer?.Name?.Replace(" ", "_")}_{claim.ClaimId:N}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting invoice");
                TempData["ErrorMessage"] = "Error exporting invoice";
                return RedirectToAction(nameof(ApprovedClaims));
            }
        }

        // GET: HR/GenerateReport
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
                ViewBag.TotalAmount = claimList.Sum(c => c.TotalHours * c.HourlyRate);
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

        // GET: HR/ExportReportCSV
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
                csv.AppendLine("Claim Report - Newlands University Claims Management System");
                csv.AppendLine($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");
                csv.AppendLine();
                csv.AppendLine("Lecturer Name,Email,Claim Date,Hours,Hourly Rate,Total Amount,Approved Date,Status");

                foreach (var claim in claimList)
                {
                    var totalAmount = claim.TotalHours * claim.HourlyRate;
                    var lecturerName = claim.Lecturer?.Name ?? "Unknown";
                    var lecturerEmail = claim.Lecturer?.Email ?? "";
                    csv.AppendLine($"{lecturerName},{lecturerEmail},{claim.ClaimDate:dd/MM/yyyy},{claim.TotalHours:F1},R {claim.HourlyRate:N2},R {totalAmount:N2},{claim.ApprovedDate:dd/MM/yyyy},{claim.ClaimStatus}");
                }

                csv.AppendLine();
                csv.AppendLine($"Total Claims,{claimList.Count}");
                csv.AppendLine($"Total Hours,{claimList.Sum(c => c.TotalHours):F1}");
                csv.AppendLine($"Total Amount,R {claimList.Sum(c => c.TotalHours * c.HourlyRate):N2}");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"Claims_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["ErrorMessage"] = "Error exporting report";
                return RedirectToAction(nameof(GenerateReport));
            }
        }

        // Helper method to generate invoice in CSV format
        private string GenerateInvoiceCSV(Claim claim)
        {
            var csv = new StringBuilder();
            var unknownText = "Unknown";
            var settledText = "Settled";
            var pendingText = "Pending";

            csv.AppendLine("NEWLANDS UNIVERSITY");
            csv.AppendLine("Claims Management System - Invoice");
            csv.AppendLine();
            csv.AppendLine($"Invoice Date:,{DateTime.Now:dd MMM yyyy}");
            csv.AppendLine($"Claim ID:,{claim.ClaimId:N}");
            csv.AppendLine();
            csv.AppendLine("LECTURER INFORMATION");
            csv.AppendLine($"Name:,{(claim.Lecturer?.Name ?? unknownText)}");
            csv.AppendLine($"Email:,{claim.Lecturer?.Email}");
            csv.AppendLine();
            csv.AppendLine("CLAIM DETAILS");
            csv.AppendLine($"Claim Date:,{claim.ClaimDate:dd MMM yyyy}");
            csv.AppendLine($"Submission Date:,{claim.SubmissionDate:dd MMM yyyy}");
            csv.AppendLine($"Approved Date:,{claim.ApprovedDate:dd MMM yyyy}");
            csv.AppendLine();
            csv.AppendLine("PAYMENT CALCULATION");
            csv.AppendLine($"Hours Worked:,{claim.TotalHours:F1}");
            csv.AppendLine($"Hourly Rate:,R {claim.HourlyRate:N2}");
            csv.AppendLine($"Total Amount Due:,R {(claim.TotalHours * claim.HourlyRate):N2}");
            csv.AppendLine();
            csv.AppendLine($"Status:,{claim.ClaimStatus}");
            csv.AppendLine($"Payment Status:,{(claim.IsSettled ? settledText : pendingText)}");

            return csv.ToString();
        }

        private bool LecturerExists(Guid id)
        {
            return _context.Lecturers.Any(e => e.LecturerId == id);
        }
    }
}
