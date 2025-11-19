using ST10440112_PROG6212_CCMS.Models;

namespace ST10440112_PROG6212_CCMS.Services
{
    public interface IClaimValidationService
    {
        /// <summary>
        /// Validates a claim submission for required fields and business rules
        /// </summary>
        (bool isValid, List<string> errors) ValidateClaimSubmission(Claim claim);

        /// <summary>
        /// Validates a claim for verification (coordinator level)
        /// </summary>
        (bool isValid, List<string> errors) ValidateClaimVerification(Claim claim);

        /// <summary>
        /// Validates a claim for approval (manager level)
        /// </summary>
        (bool isValid, List<string> errors) ValidateClaimApproval(Claim claim);

        /// <summary>
        /// Sanitizes and cleans user input to prevent injection attacks
        /// </summary>
        string SanitizeInput(string input);
    }
}
