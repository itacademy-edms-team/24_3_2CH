using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore; // Не забудь добавить этот using!
using Microsoft.AspNetCore.Http;

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
            // Загружаем тред вместе с его медиафайлами, комментариями, лайками и опросами
            // Используем AsNoTracking() для операций чтения, это оптимизация производительности
            return await _context.Threads
                .Include(t => t.MediaFiles)
                .Include(t => t.Likes)
                .Include(t => t.Comments.Where(c => c.ParentCommentId == null)) // Загружаем только корневые комментарии
                    .ThenInclude(c => c.MediaFiles)
                .Include(t => t.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.Likes)
                .Include(t => t.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.ChildComments)
                        .ThenInclude(cc => cc.MediaFiles)
                .Include(t => t.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.ChildComments)
                        .ThenInclude(cc => cc.Likes)
                .Include(t => t.Quizzes)
                    .ThenInclude(q => q.Options)
                        .ThenInclude(qo => qo.Votes)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
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
                    // Передаем false, чтобы не сохранять изменения после каждого комментария
                    await _commentService.DeleteCommentAsync(comment.Id, false);
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
            catch (Exception)
            {
                await transaction.RollbackAsync();
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
    }
}