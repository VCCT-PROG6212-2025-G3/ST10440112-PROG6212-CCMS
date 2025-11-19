using ST10440112_PROG6212_CCMS.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ST10440112_PROG6212_CCMS.Services
{
    public class ClaimValidationService : IClaimValidationService
    {
        private readonly ILogger<ClaimValidationService> _logger;

        public ClaimValidationService(ILogger<ClaimValidationService> logger)
        {
            _logger = logger;
        }

        public (bool isValid, List<string> errors) ValidateClaimSubmission(Claim claim)
        {
            var errors = new List<string>();

            // Required field validation
            if (claim == null)
            {
                errors.Add("Claim object cannot be null");
                return (false, errors);
            }

            if (claim.LecturerId == Guid.Empty)
            {
                errors.Add("Lecturer ID is required");
            }

            if (claim.TotalHours <= 0 || claim.TotalHours > 24)
            {
                errors.Add("Total hours must be between 0.01 and 24 hours");
            }

            if (claim.HourlyRate <= 0)
            {
                errors.Add("Hourly rate must be greater than zero");
            }

            if (claim.ClaimDate == default)
            {
                errors.Add("Claim date is required");
            }

            if (claim.ClaimDate > DateTime.Now)
            {
                errors.Add("Claim date cannot be in the future");
            }

            if (claim.SubmissionDate == default)
            {
                errors.Add("Submission date is required");
            }

            if (claim.SubmissionDate < claim.ClaimDate)
            {
                errors.Add("Submission date cannot be before claim date");
            }

            return (errors.Count == 0, errors);
        }

        public (bool isValid, List<string> errors) ValidateClaimVerification(Claim claim)
        {
            var errors = new List<string>();

            if (claim == null)
            {
                errors.Add("Claim object cannot be null");
                return (false, errors);
            }

            // Claim must be in Pending status to be verified
            if (claim.ClaimStatus != "Pending")
            {
                errors.Add($"Only pending claims can be verified. Current status: {claim.ClaimStatus}");
            }

            // Validate data integrity
            var (isValid, submissionErrors) = ValidateClaimSubmission(claim);
            if (!isValid)
            {
                errors.AddRange(submissionErrors);
            }

            // Check for excessive hours (more than 8 hours per day is suspicious)
            if (claim.TotalHours > 8)
            {
                _logger.LogWarning($"Claim {claim.ClaimId}: Excessive hours detected ({claim.TotalHours} hours)");
            }

            return (errors.Count == 0, errors);
        }

        public (bool isValid, List<string> errors) ValidateClaimApproval(Claim claim)
        {
            var errors = new List<string>();

            if (claim == null)
            {
                errors.Add("Claim object cannot be null");
                return (false, errors);
            }

            // Claim must be Verified to be approved
            if (claim.ClaimStatus != "Verified")
            {
                errors.Add($"Only verified claims can be approved. Current status: {claim.ClaimStatus}");
            }

            // Validate basic claim data
            var (isValid, submissionErrors) = ValidateClaimSubmission(claim);
            if (!isValid)
            {
                errors.AddRange(submissionErrors);
            }

            // Total amount shouldn't exceed reasonable limit (e.g., R 100,000)
            var totalAmount = claim.TotalAmount;
            if (totalAmount > 100000)
            {
                errors.Add($"Claim amount ({totalAmount:C}) exceeds maximum allowed");
            }

            return (errors.Count == 0, errors);
        }

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove potential script tags and dangerous characters
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"<[^>]+>", "");  // Remove HTML tags
            input = input.Replace("\"", "&quot;");          // Escape quotes
            input = input.Replace("'", "&#x27;");           // Escape single quotes
            input = input.Replace("<", "&lt;");             // Escape angle brackets
            input = input.Replace(">", "&gt;");

            return input.Trim();
        }
    }
}
