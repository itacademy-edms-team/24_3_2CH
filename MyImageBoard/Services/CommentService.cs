using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ForumProject.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripcodeService _tripcodeService;
        private readonly IMediaFileService _mediaFileService;

        public CommentService(ApplicationDbContext context, ITripcodeService tripcodeService, IMediaFileService mediaFileService)
        {
            _context = context;
            _tripcodeService = tripcodeService;
            _mediaFileService = mediaFileService;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.MediaFiles)
                .Include(c => c.Likes)
                .Include(c => c.Complaints)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetCommentsForThreadAsync(int threadId)
        {
            // Загружаем корневые комментарии для заданного треда вместе со всеми вложенными
            return await _context.Comments
                .Where(c => c.ThreadId == threadId && c.ParentCommentId == null) // Только корневые комментарии
                .Include(c => c.MediaFiles)
                .Include(c => c.Likes)
                .Include(c => c.Complaints)
                .Include(c => c.ChildComments) // Включаем дочерние комментарии
                    .ThenInclude(cc => cc.MediaFiles)
                .Include(c => c.ChildComments) // Повторяем Include для другой навигации
                    .ThenInclude(cc => cc.Likes)
                .Include(c => c.ChildComments) // И для жалоб
                    .ThenInclude(cc => cc.Complaints)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;

            // Обработка трипкода, если он есть
            if (!string.IsNullOrEmpty(comment.Tripcode))
            {
                comment.Tripcode = _tripcodeService.GenerateTripcode(comment.Tripcode);
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            var existingComment = await _context.Comments.FindAsync(comment.Id);
            if (existingComment == null)
            {
                return false;
            }

            existingComment.Content = comment.Content;
            // existingComment.HasComplaint = comment.HasComplaint; // Это будем менять через отдельный метод

            try
            {
                _context.Comments.Update(existingComment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(int id, bool saveChanges = true)
        {
            // Если saveChanges = false, не создаем транзакцию - она будет управляться извне
            if (saveChanges)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var result = await DeleteCommentInternalAsync(id);
                    if (result)
                    {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Логируем ошибку для отладки
                    System.Diagnostics.Debug.WriteLine($"Error deleting comment {id}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // Если saveChanges = false, просто выполняем удаление без транзакции
                return await DeleteCommentInternalAsync(id);
            }
        }

        private async Task<bool> DeleteCommentInternalAsync(int id)
        {
            try
            {
                // 1. Загружаем комментарий со всеми зависимостями
                var commentToDelete = await _context.Comments
                    .Include(c => c.Likes)
                    .Include(c => c.Complaints)
                    .Include(c => c.MediaFiles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (commentToDelete == null)
                {
                    return false;
                }

                // 2. Рекурсивно удаляем все дочерние комментарии
                await DeleteCommentRecursivelyAsync(commentToDelete.Id);

                // 3. Удаляем зависимости текущего комментария
                if (commentToDelete.Likes != null)
                {
                    _context.Likes.RemoveRange(commentToDelete.Likes);
                }

                if (commentToDelete.Complaints != null)
                {
                    _context.Complaints.RemoveRange(commentToDelete.Complaints);
                }

                if (commentToDelete.MediaFiles != null)
                {
                    foreach (var mediaFile in commentToDelete.MediaFiles)
                    {
                        await _mediaFileService.DeleteFileAsync(mediaFile.FileName);
                    }
                    _context.MediaFiles.RemoveRange(commentToDelete.MediaFiles);
                }

                // 4. Удаляем сам комментарий
                _context.Comments.Remove(commentToDelete);

                return true;
            }
            catch (Exception ex)
            {
                // Логируем ошибку для отладки
                System.Diagnostics.Debug.WriteLine($"Error in DeleteCommentInternalAsync for comment {id}: {ex.Message}");
                throw;
            }
        }

        private async Task DeleteCommentRecursivelyAsync(int commentId)
        {
            // Загружаем все дочерние комментарии для данного комментария
            var childComments = await _context.Comments
                .Include(c => c.Likes)
                .Include(c => c.Complaints)
                .Include(c => c.MediaFiles)
                .Where(c => c.ParentCommentId == commentId)
                .ToListAsync();

            foreach (var childComment in childComments)
            {
                // Рекурсивно удаляем дочерние комментарии этого дочернего комментария
                await DeleteCommentRecursivelyAsync(childComment.Id);

                // Удаляем зависимости дочернего комментария
                if (childComment.Likes != null)
                {
                    _context.Likes.RemoveRange(childComment.Likes);
                }
                if (childComment.Complaints != null)
                {
                    _context.Complaints.RemoveRange(childComment.Complaints);
                }
                if (childComment.MediaFiles != null)
                {
                    foreach (var mediaFile in childComment.MediaFiles)
                    {
                        await _mediaFileService.DeleteFileAsync(mediaFile.FileName);
                    }
                    _context.MediaFiles.RemoveRange(childComment.MediaFiles);
                }

                // Удаляем сам дочерний комментарий
                _context.Comments.Remove(childComment);
            }
        }
    }
}