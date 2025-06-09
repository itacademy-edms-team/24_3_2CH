using ForumProject.Data.Models;
using Microsoft.AspNetCore.Http;

namespace ForumProject.Services.Interfaces
{
    public interface IMediaFileService
    {
        Task<(bool isValid, string? errorMessage)> ValidateFilesAsync(IFormFileCollection files);
        Task<List<MediaFile>> SaveFilesAsync(IFormFileCollection files, int threadId);
        Task<List<MediaFile>> SaveCommentFilesAsync(IFormFileCollection files, int commentId, int threadId);
        Task DeleteThreadFilesAsync(int threadId);
        Task<bool> DeleteFileAsync(string filePath);
    }
} 