namespace ST10440112_PROG6212_CCMS.Services
{
    /// <summary>
    /// Interface for file encryption and decryption operations
    /// </summary>
    public interface IFileEncryptionService
    {
        /// <summary>
        /// Encrypts a file at the specified path
        /// </summary>
        Task<(bool Success, string Message)> EncryptFileAsync(string filePath);

        /// <summary>
        /// Decrypts a file and returns the decrypted data
        /// </summary>
        Task<(bool Success, string Message, byte[]? DecryptedData)> DecryptFileAsync(string filePath);

        /// <summary>
        /// Encrypts a stream of data
        /// </summary>
        Task<Stream> EncryptStreamAsync(Stream inputStream);

        /// <summary>
        /// Decrypts a stream of data
        /// </summary>
        Task<Stream> DecryptStreamAsync(Stream inputStream);

        /// <summary>
        /// Checks if a file is encrypted
        /// </summary>
        Task<bool> IsFileEncryptedAsync(string filePath);
    }
}
