using System.Security.Cryptography;
using System.Text;

namespace ST10440112_PROG6212_CCMS.Services
{
    /// <summary>
    /// Service for encrypting and decrypting files using AES-256 encryption
    /// </summary>
    public class FileEncryptionService : IFileEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileEncryptionService> _logger;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _iv;

        public FileEncryptionService(IConfiguration configuration, ILogger<FileEncryptionService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Get encryption key from configuration or generate one
            var keyString = _configuration["FileEncryption:Key"] ?? GenerateSecureKey();
            var ivString = _configuration["FileEncryption:IV"] ?? GenerateSecureIV();

            _encryptionKey = Convert.FromBase64String(keyString);
            _iv = Convert.FromBase64String(ivString);

            // Log warning if using default keys (should use appsettings in production)
            if (_configuration["FileEncryption:Key"] == null)
            {
                _logger.LogWarning("Using generated encryption key. For production, set FileEncryption:Key in appsettings.json");
            }
        }

        /// <summary>
        /// Encrypts a file using AES-256 encryption
        /// </summary>
        public async Task<(bool Success, string Message)> EncryptFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, "File not found.");
                }

                // Read the original file
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                // Encrypt the file
                byte[] encryptedBytes = EncryptBytes(fileBytes);

                // Overwrite the file with encrypted content
                await File.WriteAllBytesAsync(filePath, encryptedBytes);

                _logger.LogInformation($"File encrypted successfully: {Path.GetFileName(filePath)}");
                return (true, "File encrypted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error encrypting file: {filePath}");
                return (false, $"Encryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrypts a file using AES-256 decryption
        /// </summary>
        public async Task<(bool Success, string Message, byte[]? DecryptedData)> DecryptFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return (false, "File not found.", null);
                }

                // Read the encrypted file
                byte[] encryptedBytes = await File.ReadAllBytesAsync(filePath);

                // Decrypt the file
                byte[] decryptedBytes = DecryptBytes(encryptedBytes);

                _logger.LogInformation($"File decrypted successfully: {Path.GetFileName(filePath)}");
                return (true, "File decrypted successfully.", decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error decrypting file: {filePath}");
                return (false, $"Decryption failed: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Encrypts a stream of data
        /// </summary>
        public async Task<Stream> EncryptStreamAsync(Stream inputStream)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var outputStream = new MemoryStream();

                using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write, leaveOpen: true))
                {
                    await inputStream.CopyToAsync(cryptoStream);
                }

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting stream");
                throw;
            }
        }

        /// <summary>
        /// Decrypts a stream of data
        /// </summary>
        public async Task<Stream> DecryptStreamAsync(Stream inputStream)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var outputStream = new MemoryStream();

                using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                {
                    await cryptoStream.CopyToAsync(outputStream);
                }

                outputStream.Position = 0;
                return outputStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting stream");
                throw;
            }
        }

        /// <summary>
        /// Encrypts byte array using AES-256
        /// </summary>
        private byte[] EncryptBytes(byte[] plainBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        /// <summary>
        /// Decrypts byte array using AES-256
        /// </summary>
        private byte[] DecryptBytes(byte[] encryptedBytes)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            return decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        }

        /// <summary>
        /// Generates a secure 256-bit encryption key
        /// </summary>
        private string GenerateSecureKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        /// <summary>
        /// Generates a secure initialization vector
        /// </summary>
        private string GenerateSecureIV()
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            return Convert.ToBase64String(aes.IV);
        }

        /// <summary>
        /// Verifies if a file is encrypted
        /// </summary>
        public async Task<bool> IsFileEncryptedAsync(string filePath)
        {
            try
            {
                // Try to decrypt - if it fails, it's not encrypted or corrupted
                var result = await DecryptFileAsync(filePath);
                return result.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
