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

        public async Task<int> AddLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                l.LikeTypeId == likeTypeId &&
                (threadId.HasValue && l.ThreadId == threadId || commentId.HasValue && l.CommentId == commentId));

            if (existingLike == null)
            {
                var newLike = new Like
                {
                    UserFingerprintId = userFingerprintId,
                    ThreadId = threadId,
                    CommentId = commentId,
                    LikeTypeId = likeTypeId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(newLike);
                await _context.SaveChangesAsync();
            }

            // Возвращаем количество лайков данного типа после добавления (или если уже был)
            return await GetLikeCountAsync(threadId, commentId, likeTypeId);
        }

        public async Task<bool> RemoveLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId)
        {
            var likeToRemove = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                l.LikeTypeId == likeTypeId &&
                (threadId.HasValue && l.ThreadId == threadId || commentId.HasValue && l.CommentId == commentId));

            if (likeToRemove != null)
            {
                _context.Likes.Remove(likeToRemove);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ToggleLikeAsync(int userFingerprintId, int? threadId, int? commentId, int likeTypeId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l =>
                l.UserFingerprintId == userFingerprintId &&
                l.LikeTypeId == likeTypeId &&
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
                    LikeTypeId = likeTypeId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(newLike);
                await _context.SaveChangesAsync();
                return true; // Лайк добавлен
            }
        }

        public async Task<int> GetLikeCountAsync(int? threadId, int? commentId, int? likeTypeId = null)
        {
            var query = _context.Likes.AsQueryable();

            if (threadId.HasValue)
            {
                query = query.Where(l => l.ThreadId == threadId.Value);
            }
            else if (commentId.HasValue)
            {
                query = query.Where(l => l.CommentId == commentId.Value);
            }

            if (likeTypeId.HasValue)
            {
                query = query.Where(l => l.LikeTypeId == likeTypeId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> HasUserLikedAsync(int userFingerprintId, int threadId, int? commentId, int likeTypeId)
        {
            // Если лайк для треда
            if (commentId == null || commentId == 0) // Проверяем, что это лайк именно для треда
            {
                return await _context.Likes.AnyAsync(l =>
                   l.UserFingerprintId == userFingerprintId &&
                   l.ThreadId == threadId && 
                   l.CommentId == null && 
                   l.LikeTypeId == likeTypeId); // CommentId должен быть null для лайка треда
            }
            else // Лайк для комментария
            {
                return await _context.Likes.AnyAsync(l =>
                    l.UserFingerprintId == userFingerprintId &&
                    l.CommentId == commentId && 
                    l.ThreadId == null && 
                    l.LikeTypeId == likeTypeId); // ThreadId должен быть null для лайка комментария
            }
        }

        public async Task<Dictionary<int, int>> GetLikeCountsByTypeAsync(int? threadId, int? commentId)
        {
            var query = _context.Likes.AsQueryable();

            if (threadId.HasValue)
            {
                query = query.Where(l => l.ThreadId == threadId.Value);
            }
            else if (commentId.HasValue)
            {
                query = query.Where(l => l.CommentId == commentId.Value);
            }

            var likeCounts = await query
                .GroupBy(l => l.LikeTypeId)
                .Select(g => new { LikeTypeId = g.Key, Count = g.Count() })
                .ToListAsync();

            return likeCounts.ToDictionary(x => x.LikeTypeId, x => x.Count);
        }

        public async Task<Dictionary<int, bool>> GetUserReactionsForThreadAsync(int userFingerprintId, int threadId)
        {
            var userReactions = await _context.Likes
                .Where(l => l.UserFingerprintId == userFingerprintId && l.ThreadId == threadId)
                .Select(l => l.LikeTypeId)
                .ToListAsync();

            // Получаем все активные типы реакций
            var allLikeTypes = await _context.LikeTypes
                .Where(lt => lt.IsActive)
                .Select(lt => lt.Id)
                .ToListAsync();

            var result = new Dictionary<int, bool>();
            foreach (var likeTypeId in allLikeTypes)
            {
                result[likeTypeId] = userReactions.Contains(likeTypeId);
            }

            return result;
        }

        public async Task<Dictionary<int, int>> GetReactionCountsForThreadAsync(int threadId)
        {
            var reactionCounts = await _context.Likes
                .Where(l => l.ThreadId == threadId)
                .GroupBy(l => l.LikeTypeId)
                .Select(g => new { LikeTypeId = g.Key, Count = g.Count() })
                .ToListAsync();

            return reactionCounts.ToDictionary(x => x.LikeTypeId, x => x.Count);
        }

        public async Task<Dictionary<int, Dictionary<int, bool>>> GetUserReactionsForCommentsAsync(int userFingerprintId, ICollection<Comment> comments)
        {
            if (comments == null || !comments.Any())
                return new Dictionary<int, Dictionary<int, bool>>();

            var commentIds = comments.Select(c => c.Id).ToList();
            
            var userReactions = await _context.Likes
                .Where(l => l.UserFingerprintId == userFingerprintId && 
                           l.CommentId.HasValue && 
                           commentIds.Contains(l.CommentId.Value))
                .Select(l => new { l.CommentId, l.LikeTypeId })
                .ToListAsync();

            var allLikeTypes = await _context.LikeTypes
                .Where(lt => lt.IsActive)
                .Select(lt => lt.Id)
                .ToListAsync();

            var result = new Dictionary<int, Dictionary<int, bool>>();

            foreach (var comment in comments)
            {
                var commentReactions = userReactions
                    .Where(ur => ur.CommentId == comment.Id)
                    .Select(ur => ur.LikeTypeId)
                    .ToList();

                var commentResult = new Dictionary<int, bool>();
                foreach (var likeTypeId in allLikeTypes)
                {
                    commentResult[likeTypeId] = commentReactions.Contains(likeTypeId);
                }

                result[comment.Id] = commentResult;
            }

            return result;
        }

        public async Task<Dictionary<int, Dictionary<int, int>>> GetReactionCountsForCommentsAsync(ICollection<Comment> comments)
        {
            if (comments == null || !comments.Any())
                return new Dictionary<int, Dictionary<int, int>>();

            var commentIds = comments.Select(c => c.Id).ToList();

            var reactionCounts = await _context.Likes
                .Where(l => l.CommentId.HasValue && commentIds.Contains(l.CommentId.Value))
                .GroupBy(l => new { l.CommentId, l.LikeTypeId })
                .Select(g => new { g.Key.CommentId, g.Key.LikeTypeId, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<int, Dictionary<int, int>>();

            foreach (var comment in comments)
            {
                var commentCounts = reactionCounts
                    .Where(rc => rc.CommentId == comment.Id)
                    .ToDictionary(rc => rc.LikeTypeId, rc => rc.Count);

                result[comment.Id] = commentCounts;
            }

            return result;
        }
    }
}