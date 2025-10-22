using System.Text.RegularExpressions;
using System.Web;

namespace ST10440112_PROG6212_CCMS.Helpers
{
    /// <summary>
    /// Helper class for sanitizing user input to prevent XSS and injection attacks
    /// </summary>
    public static class InputSanitizer
    {
        /// <summary>
        /// Sanitizes text input by encoding HTML and removing dangerous characters
        /// </summary>
        public static string SanitizeText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Trim whitespace
            input = input.Trim();

            // Remove any HTML tags
            input = Regex.Replace(input, @"<[^>]*>", string.Empty);

            // Encode HTML entities
            input = HttpUtility.HtmlEncode(input);

            // Remove potentially dangerous characters
            input = Regex.Replace(input, @"[<>""']", string.Empty);

            return input;
        }

        /// <summary>
        /// Sanitizes comment text while preserving line breaks
        /// </summary>
        public static string SanitizeComment(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Trim and normalize line breaks
            input = input.Trim();
            input = Regex.Replace(input, @"\r\n|\r|\n", "\n");

            // Remove HTML tags
            input = Regex.Replace(input, @"<[^>]*>", string.Empty);

            // Remove script tags and their content
            input = Regex.Replace(input, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", string.Empty, RegexOptions.IgnoreCase);

            // Encode dangerous characters
            input = input.Replace("<", "&lt;").Replace(">", "&gt;");

            return input;
        }

        /// <summary>
        /// Validates and sanitizes file names
        /// </summary>
        public static string SanitizeFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            // Remove path information
            fileName = Path.GetFileName(fileName);

            // Remove invalid file name characters
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Remove potentially dangerous patterns
            fileName = Regex.Replace(fileName, @"\.\.", string.Empty);
            fileName = Regex.Replace(fileName, @"[<>:""/\\|?*]", string.Empty);

            // Limit length
            if (fileName.Length > 255)
            {
                var extension = Path.GetExtension(fileName);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                fileName = nameWithoutExt.Substring(0, 255 - extension.Length) + extension;
            }

            return fileName;
        }

        /// <summary>
        /// Validates numeric input
        /// </summary>
        public static bool IsValidNumeric(string? input, out decimal value)
        {
            value = 0;
            
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            // Remove whitespace and common separators
            input = input.Trim().Replace(" ", "").Replace(",", "");

            return decimal.TryParse(input, out value) && value >= 0;
        }

        /// <summary>
        /// Validates and sanitizes email addresses
        /// </summary>
        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes excessive whitespace from text
        /// </summary>
        public static string NormalizeWhitespace(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // Replace multiple spaces with single space
            input = Regex.Replace(input, @"\s+", " ");

            // Trim
            return input.Trim();
        }

        /// <summary>
        /// Validates Guid format
        /// </summary>
        public static bool IsValidGuid(string? input, out Guid guid)
        {
            guid = Guid.Empty;
            
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Guid.TryParse(input, out guid);
        }
    }
}
