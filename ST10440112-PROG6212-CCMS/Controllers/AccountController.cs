using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ST10440112_PROG6212_CCMS.Models;
using ST10440112_PROG6212_CCMS.Services;
using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ISessionManagementService _sessionManagementService;
        private readonly ILoginRedirectHelper _redirectHelper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ISessionManagementService sessionManagementService, ILoginRedirectHelper redirectHelper, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sessionManagementService = sessionManagementService;
            _redirectHelper = redirectHelper;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find user by email first
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Try to sign in with the username
                    var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        // Add claims to the user
                        var claims = new List<System.Security.Claims.Claim>
                        {
                            new System.Security.Claims.Claim("FullName", user.FullName ?? user.UserName ?? ""),
                            new System.Security.Claims.Claim("Email", user.Email ?? "")
                        };
                        await _userManager.AddClaimsAsync(user, claims);

                        // Create session in database
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                        var userSession = await _sessionManagementService.CreateSessionAsync(
                            user.Id, user.Email ?? "", user.Role ?? "", ipAddress);

                        // Log activity
                        await _sessionManagementService.LogActivityAsync(
                            user.Id, user.Email ?? "", user.Role ?? "", "Login",
                            "Account", "Login", ipAddress, true, $"User logged in successfully");

                        HttpContext.Session.SetString("UserId", user.Id);
                        HttpContext.Session.SetString("UserEmail", user.Email ?? "");
                        HttpContext.Session.SetString("UserName", user.FullName ?? "");
                        HttpContext.Session.SetString("UserRole", user.Role ?? "");
                        HttpContext.Session.SetString("SessionId", userSession.SessionId.ToString());

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        // Log failed login attempt
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                        await _sessionManagementService.LogActivityAsync(
                            user.Id, user.Email ?? "", user.Role ?? "", "Login",
                            "Account", "Login", ipAddress, false, "Failed login attempt");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get session ID from session
                if (HttpContext.Session.TryGetValue("SessionId", out var sessionIdBytes))
                {
                    var sessionIdStr = System.Text.Encoding.UTF8.GetString(sessionIdBytes);
                    if (Guid.TryParse(sessionIdStr, out var sessionId))
                    {
                        // End session in database
                        await _sessionManagementService.EndSessionAsync(sessionId);

                        // Log activity
                        var userId = HttpContext.Session.GetString("UserId") ?? "";
                        var email = HttpContext.Session.GetString("UserEmail") ?? "";
                        var role = HttpContext.Session.GetString("UserRole") ?? "";
                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                        await _sessionManagementService.LogActivityAsync(
                            userId, email, role, "Logout",
                            "Account", "Logout", ipAddress, true, "User logged out");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }

            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Get user role from session or claims
            var userRole = HttpContext.Session.GetString("UserRole");
            var dashboardRoute = _redirectHelper.GetDashboardRoute(userRole);

            return RedirectToAction(dashboardRoute.Action, dashboardRoute.Controller);
        }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
