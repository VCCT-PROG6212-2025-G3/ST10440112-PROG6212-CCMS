using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

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

        private bool LecturerExists(Guid id)
        {
            return _context.Lecturers.Any(e => e.LecturerId == id);
        }
    }
}
