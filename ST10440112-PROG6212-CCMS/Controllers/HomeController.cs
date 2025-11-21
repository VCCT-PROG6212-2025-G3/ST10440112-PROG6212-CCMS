using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10440112_PROG6212_CCMS.Data;
using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Controllers;

[Authorize(Roles = "Lecturer")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get the current user and their lecturer profile
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.Email == user.Email);
            if (lecturer == null)
            {
                return View(new List<Claim>());
            }

            // Get lecturer's claims statistics
            var totalClaims = await _context.Claims.CountAsync(c => c.LecturerId == lecturer.LecturerId);
            var approvedClaims = await _context.Claims.CountAsync(c => c.LecturerId == lecturer.LecturerId && c.ClaimStatus == "Approved");
            var pendingClaims = await _context.Claims.CountAsync(c => c.LecturerId == lecturer.LecturerId && (c.ClaimStatus == "Pending" || c.ClaimStatus == "Verified"));
            var rejectedClaims = await _context.Claims.CountAsync(c => c.LecturerId == lecturer.LecturerId && c.ClaimStatus == "Rejected");
            var totalAmount = await _context.Claims
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .SumAsync(c => (decimal)c.TotalHours * c.HourlyRate);

            ViewBag.TotalClaims = totalClaims;
            ViewBag.ApprovedClaims = approvedClaims;
            ViewBag.PendingClaims = pendingClaims;
            ViewBag.RejectedClaims = rejectedClaims;
            ViewBag.TotalAmount = totalAmount;

            // Get recent claims
            var recentClaims = await _context.Claims
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .Include(c => c.Documents)
                .Include(c => c.Comments)
                .OrderByDescending(c => c.SubmissionDate)
                .Take(7)
                .ToListAsync();

            return View(recentClaims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lecturer dashboard");
            return View(new List<Claim>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
