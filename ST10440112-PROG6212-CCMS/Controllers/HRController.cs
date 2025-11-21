using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<HRController> _logger;

        public HRController(ApplicationDbContext context, UserManager<AppUser> userManager, ILogger<HRController> logger)
        {
            _context = context;
            _userManager = userManager;
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
                    .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

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

        // GET: HR/CreateLecturer
        public IActionResult CreateLecturer()
        {
            return View();
        }

        // POST: HR/CreateLecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    if (await _context.Lecturers.AnyAsync(l => l.Email == lecturer.Email))
                    {
                        ModelState.AddModelError("Email", "A lecturer with this email already exists.");
                        return View(lecturer);
                    }

                    lecturer.LecturerId = Guid.NewGuid();
                    _context.Add(lecturer);
                    await _context.SaveChangesAsync();

                    // Create AppUser login
                    var user = new AppUser
                    {
                        UserName = lecturer.Email,
                        Email = lecturer.Email,
                        FullName = lecturer.Name,
                        Role = "Lecturer",
                        LecturerId = lecturer.LecturerId,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, "Password123!"); // Default password
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Lecturer");
                        TempData["SuccessMessage"] = $"Lecturer '{lecturer.Name}' added successfully with default password 'Password123!'";
                    }
                    else
                    {
                        TempData["WarningMessage"] = $"Lecturer added but login creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    }

                    return RedirectToAction(nameof(ManageLecturers));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating lecturer");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the lecturer.");
                }
            }
            return View(lecturer);
        }

        // GET: HR/CreateUser
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new List<string> { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };
            return View();
        }

        // POST: HR/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string role, decimal? hourlyRate)
        {
            ViewBag.Roles = new List<string> { "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View();
            }

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("email", "A user with this email already exists.");
                    return View();
                }

                // If role is Lecturer, create Lecturer profile first
                Guid? lecturerId = null;
                if (role == "Lecturer")
                {
                    if (!hourlyRate.HasValue || hourlyRate.Value <= 0)
                    {
                        ModelState.AddModelError("hourlyRate", "Hourly rate is required for lecturers and must be greater than 0.");
                        return View();
                    }

                    // Check if lecturer profile exists
                    if (await _context.Lecturers.AnyAsync(l => l.Email == email))
                    {
                        ModelState.AddModelError("email", "A lecturer with this email already exists.");
                        return View();
                    }

                    var lecturer = new Lecturer
                    {
                        LecturerId = Guid.NewGuid(),
                        Name = fullName,
                        Email = email,
                        Department = "Unspecified", // Can be updated later
                        HourlyRate = hourlyRate.Value
                    };

                    _context.Lecturers.Add(lecturer);
                    await _context.SaveChangesAsync();
                    lecturerId = lecturer.LecturerId;
                }

                // Create AppUser
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    LecturerId = lecturerId,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Password123!"); // Default password
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    
                    _logger.LogInformation($"HR created new user: {email} with role: {role}");
                    
                    TempData["SuccessMessage"] = $"User '{fullName}' created successfully as {role} with default password 'Password123!'";
                    return RedirectToAction("ManageUsers");
                }
                else
                {
                    // If user creation failed and we created a lecturer, remove it
                    if (lecturerId.HasValue)
                    {
                        var lecturer = await _context.Lecturers.FindAsync(lecturerId.Value);
                        if (lecturer != null)
                        {
                            _context.Lecturers.Remove(lecturer);
                            await _context.SaveChangesAsync();
                        }
                    }

                    ModelState.AddModelError(string.Empty, $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
            }

            return View();
        }

        // GET: HR/ManageUsers
        public async Task<IActionResult> ManageUsers(string searchTerm = "", string roleFilter = "")
        {
            try
            {
                IQueryable<AppUser> query = _context.Users;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(u => u.Role == roleFilter);
                }

                var users = await query.OrderBy(u => u.FullName).ToListAsync();
                
                ViewBag.Roles = new List<string> { "All", "Lecturer", "ProgrammeCoordinator", "AcademicManager", "HR" };
                ViewBag.CurrentFilter = roleFilter;
                
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                return View(new List<AppUser>());
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
            csv.AppendLine($"Total Amount Due:,R {((decimal)claim.TotalHours * claim.HourlyRate):N2}");
            csv.AppendLine();
            csv.AppendLine($"Status:,{claim.ClaimStatus}");
            csv.AppendLine($"Payment Status:,{(claim.IsSettled ? settledText : pendingText)}");

            return csv.ToString();
        }

        // GET: HR/BulkImport
        public IActionResult BulkImport()
        {
            return View();
        }

        // POST: HR/ImportLecturersCSV
        [HttpPost]
        public async Task<IActionResult> ImportLecturersCSV(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a CSV file to upload";
                    return RedirectToAction(nameof(BulkImport));
                }

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "File must be a CSV file";
                    return RedirectToAction(nameof(BulkImport));
                }

                var importedCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var lineNumber = 0;
                    var headerLine = await reader.ReadLineAsync();
                    lineNumber++;

                    // Validate header: Name,Email,Department,HourlyRate
                    if (string.IsNullOrWhiteSpace(headerLine) ||
                        !headerLine.Equals("Name,Email,Department,HourlyRate", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["ErrorMessage"] = "CSV header must be: Name,Email,Department,HourlyRate";
                        return RedirectToAction(nameof(BulkImport));
                    }

                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;

                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(',');

                        if (parts.Length != 4)
                        {
                            errorCount++;
                            errors.Add($"Line {lineNumber}: Invalid number of fields (expected 4)");
                            continue;
                        }

                        var name = parts[0]?.Trim();
                        var email = parts[1]?.Trim();
                        var department = parts[2]?.Trim();
                        var hourlyRateStr = parts[3]?.Trim();

                        // Validate fields
                        if (string.IsNullOrEmpty(name))
                        {
                            errorCount++;
                            errors.Add($"Line {lineNumber}: Name is required");
                            continue;
                        }

                        if (string.IsNullOrEmpty(email))
                        {
                            errorCount++;
                            errors.Add($"Line {lineNumber}: Email is required");
                            continue;
                        }

                        if (!decimal.TryParse(hourlyRateStr, out var hourlyRate) || hourlyRate <= 0)
                        {
                            errorCount++;
                            errors.Add($"Line {lineNumber}: Hourly rate must be a valid positive number");
                            continue;
                        }

                        // Check if lecturer already exists
                        var existingLecturer = await _context.Lecturers
                            .FirstOrDefaultAsync(l => l.Email == email);

                        if (existingLecturer != null)
                        {
                            // Update existing lecturer
                            existingLecturer.Name = name;
                            existingLecturer.Department = department ?? existingLecturer.Department;
                            existingLecturer.HourlyRate = hourlyRate;
                            _context.Lecturers.Update(existingLecturer);
                            importedCount++;
                        }
                        else
                        {
                            // Create new lecturer
                            var lecturer = new Lecturer
                            {
                                LecturerId = Guid.NewGuid(),
                                Name = name,
                                Email = email,
                                Department = department ?? "Unspecified",
                                HourlyRate = hourlyRate
                            };
                            _context.Lecturers.Add(lecturer);
                            
                            // Create AppUser for new lecturer
                            var user = new AppUser
                            {
                                UserName = lecturer.Email,
                                Email = lecturer.Email,
                                FullName = lecturer.Name,
                                Role = "Lecturer",
                                LecturerId = lecturer.LecturerId,
                                EmailConfirmed = true
                            };

                            var result = await _userManager.CreateAsync(user, "Password123!");
                            if (result.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(user, "Lecturer");
                            }
                            else
                            {
                                errorCount++;
                                errors.Add($"Line {lineNumber}: Created lecturer but failed to create login: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }

                            importedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var message = $"Successfully imported {importedCount} lecturer(s)";
                if (errorCount > 0)
                {
                    message += $" with {errorCount} error(s)";
                    TempData["WarningMessage"] = message;
                    TempData["Errors"] = string.Join("\n", errors.Take(10)); // Show first 10 errors
                }
                else
                {
                    TempData["SuccessMessage"] = message;
                }

                return RedirectToAction(nameof(ManageLecturers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV");
                TempData["ErrorMessage"] = $"Error importing file: {ex.Message}";
                return RedirectToAction(nameof(BulkImport));
            }
        }

        // GET: HR/ExportLecturersTemplate
        public IActionResult ExportLecturersTemplate()
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Name,Email,Department,HourlyRate");
                csv.AppendLine("Michael Jones,michael.jones@example.com,Faculty of Science,350");
                csv.AppendLine("Jane Smith,jane.smith@example.com,Faculty of Arts,320");

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", "LecturersTemplate.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting template");
                TempData["ErrorMessage"] = "Error exporting template";
                return RedirectToAction(nameof(BulkImport));
            }
        }

        private bool LecturerExists(Guid id)
        {
            return _context.Lecturers.Any(e => e.LecturerId == id);
        }
    }
}
