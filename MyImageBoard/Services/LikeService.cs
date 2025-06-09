using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services.Implementations
{
    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context; // Используем ApplicationDbContext
        private readonly IHttpContextAccessor _httpContextAccessor; // Для доступа к UserFingerprintService

        public LikeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddLikeAsync(int userFingerprintId, int? threadId, int? commentId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                (threadId.HasValue && l.ThreadId == threadId || commentId.HasValue && l.CommentId == commentId));

            if (existingLike == null)
            {
                var newLike = new Like
                {
                    UserFingerprintId = userFingerprintId,
                    ThreadId = threadId,
                    CommentId = commentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(newLike);
                await _context.SaveChangesAsync();
            }

            // Возвращаем количество лайков после добавления (или если уже был)
            return await GetLikeCountAsync(threadId, commentId);
        }

        public async Task<bool> RemoveLikeAsync(int userFingerprintId, int? threadId, int? commentId)
        {
            var likeToRemove = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                (threadId.HasValue && l.ThreadId == threadId || commentId.HasValue && l.CommentId == commentId));

            if (likeToRemove != null)
            {
                _context.Likes.Remove(likeToRemove);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ToggleLikeAsync(int userFingerprintId, int? threadId, int? commentId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                (threadId.HasValue && l.ThreadId == threadId || commentId.HasValue && l.CommentId == commentId));

            if (existingLike != null)
            {
                // Лайк уже есть, удаляем его
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false; // Лайк убран
            }
            else
            {
                // Лайка нет, добавляем его
                var newLike = new Like
                {
                    UserFingerprintId = userFingerprintId,
                    ThreadId = threadId,
                    CommentId = commentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(newLike);
                await _context.SaveChangesAsync();
                return true; // Лайк добавлен
            }
        }

        public async Task<int> GetLikeCountAsync(int? threadId, int? commentId)
        {
            if (threadId.HasValue)
            {
                return await _context.Likes.CountAsync(l => l.ThreadId == threadId.Value);
            }
            if (commentId.HasValue)
            {
                return await _context.Likes.CountAsync(l => l.CommentId == commentId.Value);
            }
            return 0; // Если ни threadId, ни commentId не указаны
        }

        public async Task<bool> HasUserLikedAsync(int userFingerprintId, int threadId, int? commentId)
        {
            // Если лайк для треда
            if (commentId == null || commentId == 0) // Проверяем, что это лайк именно для треда
            {
                return await _context.Likes.AnyAsync(l =>
                   l.UserFingerprintId == userFingerprintId &&
                   l.ThreadId == threadId && l.CommentId == null); // CommentId должен быть null для лайка треда
            }
            else // Лайк для комментария
            {
                return await _context.Likes.AnyAsync(l =>
                    l.UserFingerprintId == userFingerprintId &&
                    l.CommentId == commentId && l.ThreadId == null); // ThreadId должен быть null для лайка комментария
            }
        }
    }
}