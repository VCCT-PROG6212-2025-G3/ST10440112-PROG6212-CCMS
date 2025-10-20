namespace ST10440112_PROG6212_CCMS.Services
{
    public interface IFileUploadService
    {
        Task<(bool Success, string Message, string? FilePath)> UploadFileAsync(IFormFile file, string claimId);
        bool DeleteFile(string filePath);
        bool IsValidFileType(string fileName);
        bool IsValidFileSize(long fileSize);
        string GetFileExtension(string fileName);
    }
}
