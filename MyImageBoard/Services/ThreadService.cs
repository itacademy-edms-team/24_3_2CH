using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore; // Не забудь добавить этот using!
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ForumProject.Services
{
    public class ThreadService : IThreadService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripcodeService _tripcodeService;
        private readonly IMediaFileService _mediaFileService;
        private readonly ICommentService _commentService;

        public ThreadService(
            ApplicationDbContext context, 
            ITripcodeService tripcodeService, 
            IMediaFileService mediaFileService,
            ICommentService commentService)
        {
            _context = context;
            _tripcodeService = tripcodeService;
            _mediaFileService = mediaFileService;
            _commentService = commentService;
        }

        public async Task<SiteThread?> GetThreadByIdAsync(int id)
        {
            // Загружаем тред с базовой информацией
            var thread = await _context.Threads
                .Include(t => t.MediaFiles)
                .Include(t => t.Likes)
                .Include(t => t.Quizzes)
                    .ThenInclude(q => q.Options)
                        .ThenInclude(qo => qo.Votes)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null)
                return null;

            // Рекурсивно загружаем все комментарии с полной вложенностью
            thread.Comments = await LoadCommentsRecursivelyAsync(id);

            return thread;
        }

        private async Task<ICollection<Comment>> LoadCommentsRecursivelyAsync(int threadId, int? parentCommentId = null)
        {
            var comments = await _context.Comments
                .Include(c => c.MediaFiles)
                .Include(c => c.Likes)
                .Where(c => c.ThreadId == threadId && c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            // Рекурсивно загружаем дочерние комментарии для каждого комментария
            foreach (var comment in comments)
            {
                comment.ChildComments = await LoadCommentsRecursivelyAsync(threadId, comment.Id);
            }

            return comments;
        }

        public async Task<IEnumerable<SiteThread>> GetAllThreadsAsync()
        {
            // Для главной страницы можем загружать только основную информацию о тредах
            return await _context.Threads
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(SiteThread? thread, string? errorMessage)> CreateThreadAsync(SiteThread thread, IFormFileCollection? files = null)
        {
            // Валидация основных полей треда
            if (string.IsNullOrWhiteSpace(thread.Title))
                return (null, "Заголовок треда не может быть пустым");
            
            if (string.IsNullOrWhiteSpace(thread.Content))
                return (null, "Содержимое треда не может быть пустым");

            if (thread.Title.Length > 256)
                return (null, "Заголовок треда не может быть длиннее 256 символов");

            // Проверяем существование доски
            var boardExists = await _context.Boards.AnyAsync(b => b.Id == thread.BoardId);
            if (!boardExists)
                return (null, "Указанная доска не существует");

            // Валидация и сохранение файлов, если они есть
            if (files != null && files.Count > 0)
            {
                var (isValid, errorMessage) = await _mediaFileService.ValidateFilesAsync(files);
                if (!isValid)
                    return (null, errorMessage);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                thread.CreatedAt = DateTime.UtcNow;
            
                // Обработка трипкода, если он есть
                if (!string.IsNullOrEmpty(thread.Tripcode))
                {
                    thread.Tripcode = _tripcodeService.GenerateTripcode(thread.Tripcode);
                }

                _context.Threads.Add(thread);
                await _context.SaveChangesAsync();

                // Сохраняем файлы, если они есть
                if (files != null && files.Count > 0)
                {
                    var mediaFiles = await _mediaFileService.SaveFilesAsync(files, thread.Id);
                    thread.MediaFiles = mediaFiles;
                }

                await transaction.CommitAsync();
                return (thread, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (null, $"Ошибка при создании треда: {ex.Message}");
            }
        }

        public async Task<List<SiteThread>> GetThreadsByBoardIdAsync(int boardId)
        {
            return await _context.Threads
                .Where(t => t.BoardId == boardId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateThreadAsync(SiteThread thread)
        {
            // Сначала пытаемся найти тред, чтобы убедиться, что он существует
            var existingThread = await _context.Threads.FindAsync(thread.Id);
            if (existingThread == null)
            {
                return false; // Тред не найден
            }

            // Обновляем только те свойства, которые могут быть изменены
            existingThread.Title = thread.Title;
            existingThread.Content = thread.Content;
            // existingThread.HasComplaint = thread.HasComplaint; // Это будем менять через отдельный метод
            // existingThread.ViewsCount = thread.ViewsCount; // Это будем менять через отдельный метод

            // Отслеживание изменений для связанных коллекций (MediaFiles, Comments, Quizzes)
            // потребует более сложной логики (добавление новых, удаление существующих, обновление)
            // Для простоты на данном этапе, мы не обрабатываем изменения в связанных коллекциях напрямую здесь.
            // Это будет сделано через отдельные методы или при загрузке треда.

            try
            {
                _context.Threads.Update(existingThread); // Или _context.Entry(existingThread).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Обработка конфликтов параллельного доступа, если необходимо
                return false;
            }
        }

        public async Task<bool> DeleteThreadAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Получаем все комментарии треда
                var comments = await _context.Comments
                    .Where(c => c.ThreadId == id)
                    .ToListAsync();

                // 2. Удаляем комментарии через CommentService
                foreach (var comment in comments)
                {
                    // Передаем false, чтобы НЕ сохранять изменения после каждого комментария
                    // Все изменения будут сохранены в конце одной транзакцией
                    var commentDeleted = await _commentService.DeleteCommentAsync(comment.Id, false);
                    if (!commentDeleted)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                // 3. Загружаем тред с оставшимися зависимостями
                var threadToDelete = await _context.Threads
                    .Include(t => t.MediaFiles)
                    .Include(t => t.Likes)
                    .Include(t => t.Complaints)
                    .Include(t => t.Quizzes)
                        .ThenInclude(q => q.Options)
                            .ThenInclude(o => o.Votes)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (threadToDelete == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 4. Удаляем медиафайлы треда
                if (threadToDelete.MediaFiles != null)
                {
                    foreach (var mediaFile in threadToDelete.MediaFiles)
                    {
                        await _mediaFileService.DeleteFileAsync(mediaFile.FileName);
                    }
                    _context.MediaFiles.RemoveRange(threadToDelete.MediaFiles);
                }

                // 5. Удаляем лайки треда
                if (threadToDelete.Likes != null)
                {
                    _context.Likes.RemoveRange(threadToDelete.Likes);
                }

                // 6. Удаляем жалобы на тред
                if (threadToDelete.Complaints != null)
                {
                    _context.Complaints.RemoveRange(threadToDelete.Complaints);
                }

                // 7. Удаляем опросы и их зависимости
                if (threadToDelete.Quizzes != null)
                {
                    foreach (var quiz in threadToDelete.Quizzes)
                    {
                        foreach (var option in quiz.Options)
                        {
                            _context.QuizVotes.RemoveRange(option.Votes);
                        }
                        _context.QuizOptions.RemoveRange(quiz.Options);
                    }
                    _context.Quizzes.RemoveRange(threadToDelete.Quizzes);
                }

                // 8. Удаляем сам тред
                _context.Threads.Remove(threadToDelete);

                // 9. Сохраняем все изменения одним вызовом
                await _context.SaveChangesAsync();

                // 10. Фиксируем транзакцию
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Логируем ошибку для отладки
                System.Diagnostics.Debug.WriteLine($"Error deleting thread {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AddViewsCountAsync(int id)
        {
            var thread = await _context.Threads.FindAsync(id);
            if (thread == null)
            {
                return false;
            }
            thread.ViewsCount++;
            // _context.Entry(thread).State = EntityState.Modified; // Не обязательно, если объект уже отслеживается
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ForumProject.Pages.Threads.SearchModel.ThreadSearchResult>> SearchThreadsAsync(ForumProject.Pages.Threads.SearchModel.ThreadSearchFilter filter)
        {
            var query = _context.Threads.AsQueryable();

            if (filter.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);
            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(t => t.Title.Contains(filter.Title));
            if (!string.IsNullOrWhiteSpace(filter.Content))
                query = query.Where(t => t.Content.Contains(filter.Content));
            if (!string.IsNullOrWhiteSpace(filter.Tripcode))
                query = query.Where(t => t.Tripcode.Contains(filter.Tripcode));
            if (!string.IsNullOrWhiteSpace(filter.Tag))
                query = query.Where(t => t.Tags != null && t.Tags.Contains(filter.Tag));

            // Считаем количество комментариев для каждого треда
            var threads = await query
                .Select(t => new {
                    t.Id,
                    t.Title,
                    t.CreatedAt,
                    t.Tripcode,
                    t.Tags,
                    CommentsCount = t.Comments.Count
                })
                .ToListAsync();

            // Фильтрация по количеству комментариев (после выборки, т.к. Comments.Count не всегда корректно работает в SQL)
            if (filter.CommentsFrom.HasValue)
                threads = threads.Where(t => t.CommentsCount >= filter.CommentsFrom.Value).ToList();
            if (filter.CommentsTo.HasValue)
                threads = threads.Where(t => t.CommentsCount <= filter.CommentsTo.Value).ToList();

            return threads.Select(t => new ForumProject.Pages.Threads.SearchModel.ThreadSearchResult
            {
                Id = t.Id,
                Title = t.Title,
                CreatedAt = t.CreatedAt,
                CommentsCount = t.CommentsCount,
                Tripcode = t.Tripcode,
                Tags = t.Tags
            }).ToList();
        }
    }
}