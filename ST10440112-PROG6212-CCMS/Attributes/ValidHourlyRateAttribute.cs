using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Attributes
{
    /// <summary>
    /// Validates that hourly rate is within acceptable range
    /// </summary>
    public class ValidHourlyRateAttribute : ValidationAttribute
    {
        private readonly int _minRate;
        private readonly int _maxRate;

        public ValidHourlyRateAttribute(int minRate = 100, int maxRate = 1000)
        {
            _minRate = minRate;
            _maxRate = maxRate;
            ErrorMessage = $"Hourly rate must be between R{_minRate} and R{_maxRate}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Hourly rate is required.");
            }

            decimal rate = 0;
            if (value is int i) rate = i;
            else if (value is decimal d) rate = d;
            else if (value is double db) rate = (decimal)db;
            else return new ValidationResult("Invalid rate format.");

            if (rate < _minRate || rate > _maxRate)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
