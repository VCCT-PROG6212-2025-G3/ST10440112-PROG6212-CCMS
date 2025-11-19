using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    [Authorize]
    public class ClaimReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClaimReportController> _logger;

        public ClaimReportController(ApplicationDbContext context, ILogger<ClaimReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ClaimReport/Generate/5
        public async Task<IActionResult> Generate(Guid? id)
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

                ViewBag.GeneratedDate = DateTime.Now;
                return View(claim);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating report for claim: {id}");
                TempData["ErrorMessage"] = "Error generating claim report.";
                return RedirectToAction("Index", "Claims");
            }
        }
    }
}
