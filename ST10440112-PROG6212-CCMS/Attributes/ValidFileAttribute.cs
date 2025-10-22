using System.ComponentModel.DataAnnotations;

namespace ST10440112_PROG6212_CCMS.Attributes
{
    /// <summary>
    /// Validates that file uploads meet requirements
    /// </summary>
    public class ValidFileAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" };
        private readonly long _maxFileSizeInBytes = 10 * 1024 * 1024; // 10MB

        public ValidFileAttribute()
        {
            ErrorMessage = "Invalid file. Only PDF, DOCX, and XLSX files under 10MB are allowed.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Optional file
            }

            if (value is IFormFile file)
            {
                // Check file size
                if (file.Length > _maxFileSizeInBytes)
                {
                    return new ValidationResult("File size must not exceed 10MB.");
                }

                if (file.Length == 0)
                {
                    return new ValidationResult("File is empty.");
                }

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    return new ValidationResult($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
                }

                return ValidationResult.Success;
            }

            if (value is List<IFormFile> files)
            {
                foreach (var f in files)
                {
                    var result = IsValid(f, validationContext);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid file format.");
        }
    }
}
