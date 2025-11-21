# PowerPoint Presentation Content
## Contract Monthly Claims Management System (CCMS) - Part 3

---

## Slide 1: Title Slide
**Contract Monthly Claims Management System**
Part 3: Application Enhancement & Automation

Student Number: ST10440112
Module: PROG6212
November 2025

---

## Slide 2: Agenda
1. Part 2 to Part 3 Changes Overview
2. Feedback Implementation Summary
3. Lecturer View - Claim Submission Automation
4. Coordinator & Manager View - Verification/Approval Workflow
5. HR View - User Management & Reporting
6. Session Management & Access Control
7. Document Encryption Security
8. Error Handling Improvements
9. Validation System
10. Live Demo / Key Screenshots
11. Conclusion

---

## Slide 3: Part 2 to Part 3 - What Changed?

### Part 2 Features (Existing)
- Basic claim submission
- Simple approval workflow
- Document uploads (no encryption)
- Limited error handling

### Part 3 Enhancements (New)
- ✅ ASP.NET Core Identity authentication
- ✅ HR role with user management
- ✅ Automated hourly rate (HR-managed)
- ✅ Auto-calculation (Hours × Rate)
- ✅ Two-stage approval workflow (Coordinator → Manager)
- ✅ Invoice & report generation
- ✅ Session management & access control
- ✅ AES-256 document encryption
- ✅ Circuit breaker error handling
- ✅ Comprehensive validation

---

## Slide 4: Feedback #1 - Two-Stage Approval Workflow

### Problem
- Needed better separation between verification and approval stages

### Solution Implemented
```
Pending → Coordinator Verifies → Verified → Manager Approves → Approved
                    ↓                              ↓
                Rejected                       Rejected
```

### Key Code (CoordinatorController.cs)
```csharp
// Coordinator only sees Pending claims
var pendingClaims = await _context.Claims
    .Where(c => c.ClaimStatus == "Pending")
    .Include(c => c.Lecturer)
    .ToListAsync();
```

### Key Code (ManagerController.cs)
```csharp
// Manager only sees Verified claims
var verifiedClaims = await _context.Claims
    .Where(c => c.ClaimStatus == "Verified")
    .Include(c => c.Lecturer)
    .ToListAsync();
```

**Screenshot**: Coordinator Review Modal with Verify/Reject buttons

---

## Slide 5: Feedback #2 - Lecturer Claim Submission Automation

### Problem
- Hourly rate was manually entered by lecturer (could be tampered)

### Solution: Auto-Fill Hourly Rate from HR Database

### Key Code (ClaimsController.cs)
```csharp
// Auto-fill hourly rate from lecturer profile (HR-managed)
var lecturer = await _context.Lecturers
    .FirstOrDefaultAsync(l => l.Email == userEmail);

var model = new ClaimSubmissionViewModel
{
    HourlyRate = lecturer.HourlyRate,  // Auto-filled, read-only
    TotalHours = 0
};
```

### JavaScript Auto-Calculation (Create.cshtml)
```javascript
// Real-time calculation as user types hours
$('#TotalHours').on('input', function() {
    var hours = parseFloat($(this).val()) || 0;
    var rate = parseFloat($('#HourlyRate').val()) || 0;
    var total = hours * rate;
    $('#calculatedAmount').text('R ' + total.toFixed(2));
});
```

**Screenshot**: Claim submission form with read-only hourly rate and auto-calculated total

---

## Slide 6: Feedback #3 - Document Encryption (AES-256)

### Problem
- Uploaded documents stored without encryption

### Solution: Military-Grade AES-256 Encryption

### Key Code (FileEncryptionService.cs)
```csharp
public byte[] EncryptFile(byte[] fileContent)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = _encryptionKey;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using (var encryptor = aes.CreateEncryptor())
        {
            return encryptor.TransformFinalBlock(
                fileContent, 0, fileContent.Length);
        }
    }
}
```

### Secure Download (SecureFileController.cs)
```csharp
[Authorize]
public async Task<IActionResult> Download(Guid documentId)
{
    var document = await _context.Documents.FindAsync(documentId);

    // Authorization check
    if (!await UserCanAccessDocument(document))
        return Forbid();

    // Decrypt file before serving
    var decrypted = _encryptionService.DecryptFile(document.Content);
    return File(decrypted, document.ContentType, document.FileName);
}
```

**Screenshot**: Document upload with encryption status indicator

---

## Slide 7: Feedback #4 - Error Handling (Death Loop Prevention)

### Problem
- Application entered infinite redirect loops on errors

### Solution: Circuit Breaker Pattern

### Key Code (ClaimsController.cs)
```csharp
// Track error count in session
var errorCount = HttpContext.Session.GetInt32("ErrorCount") ?? 0;

if (errorCount >= 3)
{
    // Circuit breaker triggered - break the loop
    HttpContext.Session.SetInt32("ErrorCount", 0);
    TempData["ErrorMessage"] = "Multiple errors occurred. Please try again.";
    return RedirectToAction("Index", "Home");
}

try
{
    // Normal operation
    await ProcessClaim();
    HttpContext.Session.SetInt32("ErrorCount", 0); // Reset on success
}
catch (Exception ex)
{
    HttpContext.Session.SetInt32("ErrorCount", errorCount + 1);
    _logger.LogError(ex, "Error processing claim");
    return RedirectToAction("Error", "Error");
}
```

**Screenshot**: Custom error page with helpful guidance

---

## Slide 8: HR View - User Management

### Features Implemented
- ✅ Create new users (all roles)
- ✅ Manage existing users
- ✅ Bulk import lecturers via CSV
- ✅ Update lecturer hourly rates
- ✅ No self-registration (HR creates accounts)

### Key Code (HRController.cs - CreateUser)
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserViewModel model)
{
    var user = new AppUser
    {
        UserName = model.Email.Split('@')[0],
        Email = model.Email,
        FullName = model.FullName,
        Role = model.Role
    };

    var result = await _userManager.CreateAsync(user, model.Password);

    if (result.Succeeded)
    {
        await _userManager.AddToRoleAsync(user, model.Role);

        // If Lecturer, also create Lecturer profile
        if (model.Role == "Lecturer")
        {
            var lecturer = new Lecturer
            {
                Name = model.FullName,
                Email = model.Email,
                Department = model.Department,
                HourlyRate = model.HourlyRate
            };
            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();
        }
    }
}
```

**Screenshot**: HR Dashboard with Create User and Manage Users buttons

---

## Slide 9: HR View - CSV Bulk Import

### Feature
Import multiple lecturers at once via CSV file

### CSV Format
```csv
Name,Email,Department,HourlyRate
Michael Jones,michael.jones@newlands.ac.za,Faculty of Science,350
Jane Smith,jane.smith@newlands.ac.za,Faculty of Arts,320
```

### Key Code (HRController.cs - ImportLecturersCSV)
```csharp
[HttpPost]
public async Task<IActionResult> ImportLecturersCSV(IFormFile file)
{
    using (var reader = new StreamReader(file.OpenReadStream()))
    {
        // Skip header
        await reader.ReadLineAsync();

        while ((line = await reader.ReadLineAsync()) != null)
        {
            var parts = line.Split(',');

            // Check if lecturer exists (update) or new (create)
            var existing = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.Email == parts[1]);

            if (existing != null)
            {
                existing.HourlyRate = decimal.Parse(parts[3]);
                _context.Update(existing);
            }
            else
            {
                _context.Add(new Lecturer { ... });
            }
        }
    }
    await _context.SaveChangesAsync();
}
```

**Screenshot**: Bulk Import page with CSV format instructions

---

## Slide 10: HR View - Invoice & Report Generation

### Features
- Generate invoices for approved claims
- Export reports to CSV
- Filter by date range

### Key Code (HRController.cs - GenerateInvoice)
```csharp
public async Task<IActionResult> GenerateInvoice(Guid id)
{
    var claim = await _context.Claims
        .Include(c => c.Lecturer)
        .FirstOrDefaultAsync(c => c.ClaimId == id);

    // Only approved claims can generate invoices
    if (claim.ClaimStatus != "Approved")
    {
        TempData["ErrorMessage"] = "Only approved claims can generate invoices";
        return RedirectToAction(nameof(ApprovedClaims));
    }

    return View(claim);  // Invoice view with payment details
}
```

### Key Code (ExportReportCSV)
```csharp
public async Task<IActionResult> ExportReportCSV()
{
    var csv = new StringBuilder();
    csv.AppendLine("Lecturer Name,Email,Hours,Rate,Total,Status");

    foreach (var claim in approvedClaims)
    {
        csv.AppendLine($"{claim.Lecturer.Name},{claim.Lecturer.Email}," +
            $"{claim.TotalHours},{claim.HourlyRate}," +
            $"{claim.TotalHours * claim.HourlyRate},{claim.ClaimStatus}");
    }

    return File(Encoding.UTF8.GetBytes(csv.ToString()),
        "text/csv", "ClaimReport.csv");
}
```

**Screenshot**: Invoice preview and CSV export button

---

## Slide 11: Session Management & Access Control

### Requirement
- Use sessions
- Users cannot access pages they're not supposed to

### Key Code (RoleBasedAccessFilter.cs)
```csharp
public class RoleBasedAccessFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var userRole = context.HttpContext.User
            .FindFirst(ClaimTypes.Role)?.Value;

        // Define allowed routes per role
        var allowedRoutes = new Dictionary<string, List<string>>
        {
            { "Lecturer", new List<string> { "home", "lecturer", "claim" } },
            { "ProgrammeCoordinator", new List<string> { "home", "coordinator" } },
            { "AcademicManager", new List<string> { "home", "manager" } },
            { "HR", new List<string> { "home", "hr", "lecturer" } }
        };

        // Block unauthorized access
        if (!allowedRoutes[userRole].Contains(controller.ToLower()))
        {
            context.Result = new ForbidResult();
        }
    }
}
```

### Session Creation (AccountController.cs)
```csharp
// On successful login
HttpContext.Session.SetString("UserId", user.Id);
HttpContext.Session.SetString("UserEmail", user.Email);
HttpContext.Session.SetString("UserRole", user.Role);
```

**Screenshot**: Access Denied page when unauthorized access attempted

---

## Slide 12: Validation System

### Client-Side Validation (JavaScript)
```javascript
// Real-time validation in claim form
$('#TotalHours').on('change', function() {
    var hours = parseFloat($(this).val());
    if (hours < 0.01 || hours > 24) {
        showError('Hours must be between 0.01 and 24');
    }
});
```

### Server-Side Validation (Claim.cs)
```csharp
public class Claim : IValidatableObject
{
    [Range(0.01, 24, ErrorMessage = "Hours must be between 0.01 and 24")]
    public float TotalHours { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (ClaimDate > DateTime.Now)
            yield return new ValidationResult(
                "Claim date cannot be in the future");

        if (SubmissionDate < ClaimDate)
            yield return new ValidationResult(
                "Submission cannot be before claim date");
    }
}
```

### ClaimValidationService
```csharp
public (bool isValid, List<string> errors) ValidateClaimSubmission(Claim claim)
{
    var errors = new List<string>();

    if (claim.TotalHours <= 0 || claim.TotalHours > 24)
        errors.Add("Total hours must be between 0.01 and 24");

    if (claim.HourlyRate <= 0)
        errors.Add("Hourly rate must be positive");

    if (claim.ClaimDate > DateTime.Now)
        errors.Add("Claim date cannot be in the future");

    return (errors.Count == 0, errors);
}
```

**Screenshot**: Form with validation error messages displayed

---

## Slide 13: Technology Stack Summary

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core 8.0 MVC |
| Database | SQL Server + Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Authorization | Role-based with custom filters |
| Session | HTTP Session middleware |
| Encryption | AES-256 (FIPS 140-2 compliant) |
| Validation | Data Annotations + IValidatableObject + FluentValidation |
| Reporting | LINQ + CSV export |
| Frontend | Bootstrap 5, jQuery |

---

## Slide 14: Security Features

### Authentication
- ASP.NET Core Identity with password hashing
- Secure login/logout with session management

### Authorization
- Role-based access control (4 roles)
- Custom authorization filter
- Per-controller access restrictions

### Data Protection
- AES-256 encryption for documents
- Input sanitization (XSS prevention)
- SQL injection prevention via EF Core

### Session Security
- 30-minute idle timeout
- HttpOnly cookies
- IP address tracking

---

## Slide 15: Workflow Diagram

```
                    CLAIM WORKFLOW

    ┌─────────────┐
    │   LECTURER  │
    │  Submits    │
    │   Claim     │
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │   PENDING   │
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐     ┌──────────┐
    │ COORDINATOR │────▶│ REJECTED │
    │  Verifies   │     └──────────┘
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  VERIFIED   │
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐     ┌──────────┐
    │   MANAGER   │────▶│ REJECTED │
    │  Approves   │     └──────────┘
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  APPROVED   │
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │     HR      │
    │  Processes  │
    │  Payment    │
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │   SETTLED   │
    └─────────────┘
```

---

## Slide 16: Key Screenshots

### Screenshot 1: Login Page
[Insert screenshot of login page]

### Screenshot 2: Lecturer Claim Submission
[Insert screenshot showing auto-filled hourly rate, hours input, and calculated total]

### Screenshot 3: Coordinator Review Modal
[Insert screenshot of verification modal with approve/reject buttons]

### Screenshot 4: Manager Approval Dashboard
[Insert screenshot of manager dashboard with verified claims]

### Screenshot 5: HR Dashboard
[Insert screenshot showing user management and report buttons]

### Screenshot 6: Invoice Generation
[Insert screenshot of invoice preview]

### Screenshot 7: CSV Report Export
[Insert screenshot of downloaded CSV file in Excel]

---

## Slide 17: Rubric Alignment

### Auto-calculation & Validation (18-20 marks)
- ✅ Auto-calculation: Hours × Rate computed in real-time
- ✅ Validation: Client-side + Server-side + Custom validation service
- ✅ Rate locked: HR manages hourly rates, not lecturers

### Automated Verification/Approval (18-20 marks)
- ✅ Two-stage workflow: Coordinator → Manager
- ✅ Status filtering: Each role sees only relevant claims
- ✅ Modal-based reviews with confirmation

### HR Automation (18-20 marks)
- ✅ User creation (no self-registration)
- ✅ Bulk CSV import for lecturers
- ✅ Invoice and report generation
- ✅ Session management with access control
- ✅ Document encryption

---

## Slide 18: Conclusion

### Part 3 Achievements
✅ Full authentication with ASP.NET Core Identity
✅ Role-based authorization (4 distinct roles)
✅ Automated claim submission with locked hourly rates
✅ Two-stage approval workflow
✅ HR user and lecturer management
✅ Invoice and report generation with CSV export
✅ AES-256 document encryption
✅ Circuit breaker error handling
✅ Comprehensive validation system
✅ Session management preventing unauthorized access

### All Feedback Items Addressed
✅ Feedback #1: Two-stage approval with modals
✅ Feedback #2: Auto-fill hourly rate
✅ Feedback #3: Document encryption (AES-256)
✅ Feedback #4: Death loop prevention
✅ Feedback #5: Real-time status tracking
✅ Feedback #6: Enhanced error handling

---

## Slide 19: Demo

### Live Demonstration
1. Login as Lecturer → Submit claim (auto-calculation)
2. Login as Coordinator → Verify claim
3. Login as Manager → Approve claim
4. Login as HR → Generate invoice & export CSV
5. Try unauthorized access → Access Denied

---

## Slide 20: Thank You

### Questions?

**Student**: ST10440112
**Module**: PROG6212
**Repository**: [GitHub URL]

---

# END OF PRESENTATION
