using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;

namespace MyImageBoard.Services;

public class ThreadService : IThreadService
{
    private readonly ImageBoardContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<ThreadService> _logger;

    public ThreadService(
        ImageBoardContext context,
        IUserService userService,
        ILogger<ThreadService> logger)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
    }

    public async Task<IEnumerable<ForumThread>> GetThreadsByBoardAsync(int boardId, int page = 1, int pageSize = 10)
    {
        try
        {
            return await _context.Threads
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Board)
                .Where(t => t.BoardId == boardId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting threads for board {BoardId}", boardId);
            throw;
        }
    }

    public async Task<ForumThread> GetThreadByIdAsync(int id)
    {
        try
        {
            var thread = await _context.Threads
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Board)
                .FirstOrDefaultAsync(t => t.ThreadId == id);

            if (thread is null)
            {
                throw new KeyNotFoundException($"Thread with ID {id} not found");
            }

            return thread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting thread by ID {ThreadId}", id);
            throw;
        }
    }

    public async Task<ForumThread> CreateThreadAsync(ForumThread thread, int userId)
    {
        try
        {
            // Разрешаем анонимам создавать треды
            if (userId > 0 && !await _userService.HasPermissionAsync(userId, "CreateThread"))
            {
                throw new UnauthorizedAccessException("User does not have permission to create threads");
            }

            thread.CreatedBy = userId > 0 ? userId : null;
            thread.CreatedAt = DateTime.UtcNow;

            _context.Threads.Add(thread);
            await _context.SaveChangesAsync();

            return thread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thread");
            throw;
        }
    }

    public async Task<ForumThread> UpdateThreadAsync(ForumThread thread)
    {
        try
        {
            var existingThread = await _context.Threads.FindAsync(thread.ThreadId);
            if (existingThread is null)
            {
                throw new KeyNotFoundException($"Thread with ID {thread.ThreadId} not found");
            }

            if (!await IsThreadOwnerAsync(thread.ThreadId, thread.CreatedBy!.Value) &&
                !await CanUserModerateThreadAsync(thread.ThreadId, thread.CreatedBy!.Value))
            {
                throw new UnauthorizedAccessException("User does not have permission to modify this thread");
            }

            existingThread.Title = thread.Title;
            existingThread.ImagePath = thread.ImagePath;
            existingThread.Message = thread.Message;

            await _context.SaveChangesAsync();

            return existingThread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating thread {ThreadId}", thread.ThreadId);
            throw;
        }
    }

    public async Task DeleteThreadAsync(int id, int userId)
    {
        try
        {
            var thread = await _context.Threads.FindAsync(id);
            if (thread is null)
            {
                throw new KeyNotFoundException($"Thread with ID {id} not found");
            }

            if (!await IsThreadOwnerAsync(id, userId) &&
                !await CanUserModerateThreadAsync(id, userId))
            {
                throw new UnauthorizedAccessException("User does not have permission to delete this thread");
            }

            // Удаляем связанные посты
            var posts = _context.Posts.Where(p => p.ThreadId == id);
            _context.Posts.RemoveRange(posts);
            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting thread {ThreadId}", id);
            throw;
        }
    }

    public async Task<bool> IsThreadOwnerAsync(int threadId, int userId)
    {
        try
        {
            var thread = await _context.Threads.FindAsync(threadId);
            return thread?.CreatedBy == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is owner of thread {ThreadId}", userId, threadId);
            throw;
        }
    }

    public async Task<bool> CanUserModerateThreadAsync(int threadId, int userId)
    {
        try
        {
            var thread = await _context.Threads
                .Include(t => t.Board)
                .FirstOrDefaultAsync(t => t.ThreadId == threadId);

            if (thread is null)
            {
                return false;
            }

            return await _userService.HasPermissionAsync(userId, "ModerateThread") ||
                   await _userService.HasPermissionAsync(userId, "Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can moderate thread {ThreadId}", userId, threadId);
            throw;
        }
    }

    public async Task<IEnumerable<Post>> GetThreadPostsAsync(int threadId, int page = 1, int pageSize = 20)
    {
        try
        {
            return await _context.Posts
                .Include(p => p.CreatedByNavigation)
                .Where(p => p.ThreadId == threadId)
                .OrderBy(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts for thread {ThreadId}", threadId);
            throw;
        }
    }

    public async Task<int> GetThreadPostsCountAsync(int threadId)
    {
        try
        {
            return await _context.Posts
                .CountAsync(p => p.ThreadId == threadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts count for thread {ThreadId}", threadId);
            throw;
        }
    }
} 