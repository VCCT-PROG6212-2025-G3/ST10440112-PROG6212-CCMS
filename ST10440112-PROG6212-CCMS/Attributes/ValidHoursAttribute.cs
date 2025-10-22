using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Attributes
{
    /// <summary>
    /// Validates that hours worked are within acceptable range
    /// </summary>
    public class ValidHoursAttribute : ValidationAttribute
    {
        private readonly float _minHours;
        private readonly float _maxHours;

        public ValidHoursAttribute(float minHours = 0.5f, float maxHours = 200f)
        {
            _minHours = minHours;
            _maxHours = maxHours;
            ErrorMessage = $"Hours must be between {_minHours} and {_maxHours}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Hours worked is required.");
            }

            if (value is float hours)
            {
                if (hours < _minHours || hours > _maxHours)
                {
                    return new ValidationResult(ErrorMessage);
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid hours format.");
        }
    }
}
