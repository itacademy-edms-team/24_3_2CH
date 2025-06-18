using ForumProject.Configuration;
using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public class MediaFileService : IMediaFileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MediaFileService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<(bool isValid, string? errorMessage)> ValidateFilesAsync(IFormFileCollection files)
        {
            if (files.Count > MediaFileSettings.MaxFilesPerThread)
                return (false, $"Максимальное количество файлов: {MediaFileSettings.MaxFilesPerThread}");

            long totalSize = 0;
            foreach (var file in files)
            {
                totalSize += file.Length;
                
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!MediaFileSettings.AllowedFileTypes.Contains(extension))
                    return (false, $"Неподдерживаемый тип файла: {extension}");
                
                if (!MediaFileSettings.AllowedMimeTypes.Contains(file.ContentType))
                    return (false, $"Неподдерживаемый MIME-тип: {file.ContentType}");
            }

            if (totalSize > MediaFileSettings.MaxTotalSizeBytes)
                return (false, $"Общий размер файлов превышает {MediaFileSettings.MaxTotalSizeBytes / 1024 / 1024} МБ");

            return (true, null);
        }

        public async Task<List<MediaFile>> SaveFilesAsync(IFormFileCollection files, int threadId)
        {
            var mediaFiles = new List<MediaFile>();
            var uploadPath = Path.Combine(_environment.WebRootPath, MediaFileSettings.GetThreadUploadPath(threadId));
            
            Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var mediaFile = new MediaFile
                {
                    FileName = Path.Combine(MediaFileSettings.GetThreadUploadPath(threadId), uniqueFileName).Replace("\\", "/"),
                    FileType = GetFileType(file.ContentType),
                    MimeType = file.ContentType,
                    Size = file.Length,
                    ThreadId = threadId
                };

                mediaFiles.Add(mediaFile);
                _context.MediaFiles.Add(mediaFile);
            }

            await _context.SaveChangesAsync();
            return mediaFiles;
        }

        public async Task<List<MediaFile>> SaveCommentFilesAsync(IFormFileCollection files, int commentId, int threadId)
        {
            var mediaFiles = new List<MediaFile>();
            var uploadPath = Path.Combine(_environment.WebRootPath, MediaFileSettings.GetThreadUploadPath(threadId));
            
            Directory.CreateDirectory(uploadPath);

            foreach (var file in files)
            {
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var mediaFile = new MediaFile
                {
                    FileName = Path.Combine(MediaFileSettings.GetThreadUploadPath(threadId), uniqueFileName).Replace("\\", "/"),
                    FileType = GetFileType(file.ContentType),
                    MimeType = file.ContentType,
                    Size = file.Length,
                    CommentId = commentId
                };

                mediaFiles.Add(mediaFile);
                _context.MediaFiles.Add(mediaFile);
            }

            await _context.SaveChangesAsync();
            return mediaFiles;
        }

        private string GetFileType(string mimeType)
        {
            if (mimeType.StartsWith("image/"))
            {
                return mimeType == "image/gif" ? "gif" : "image";
            }
            else if (mimeType.StartsWith("video/"))
            {
                return "video";
            }
            return "other";
        }

        public async Task DeleteThreadFilesAsync(int threadId)
        {
            var mediaFiles = await _context.MediaFiles
                .Where(m => m.ThreadId == threadId)
                .ToListAsync();

            foreach (var mediaFile in mediaFiles)
            {
                await DeleteFileAsync(mediaFile.FileName);
            }

            var threadDirectory = Path.Combine(_environment.WebRootPath, MediaFileSettings.GetThreadUploadPath(threadId));
            if (Directory.Exists(threadDirectory))
            {
                Directory.Delete(threadDirectory, true);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем процесс
                System.Diagnostics.Debug.WriteLine($"Error deleting file {fileName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Удаляет медиафайлы для указанного треда или комментария
        /// </summary>
        public async Task DeleteMediaFilesAsync(int? threadId = null, int? commentId = null)
        {
            if (!threadId.HasValue && !commentId.HasValue)
                return;

            var query = _context.MediaFiles.AsQueryable();
            
            if (threadId.HasValue)
                query = query.Where(m => m.ThreadId == threadId.Value);
            else if (commentId.HasValue)
                query = query.Where(m => m.CommentId == commentId.Value);

            var mediaFiles = await query.ToListAsync();

            foreach (var mediaFile in mediaFiles)
            {
                await DeleteFileAsync(mediaFile.FileName);
            }

            _context.MediaFiles.RemoveRange(mediaFiles);
        }
    }
} 