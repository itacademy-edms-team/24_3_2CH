using Microsoft.EntityFrameworkCore;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MyImageBoard.Services
{
    public class ModerationService : IModerationService
    {
        private readonly ImageBoardContext _context;
        private readonly ILogger<ModerationService> _logger;

        public ModerationService(ImageBoardContext context, ILogger<ModerationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ForumThread>> GetReportedThreadsAsync()
        {
            return await _context.Threads
                .Include(t => t.Board)
                .Where(t => t.IsReported)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetReportedPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.Thread)
                .ThenInclude(t => t.Board)
                .Where(p => p.IsReported)
                .ToListAsync();
        }

        public async Task ClearThreadReportAsync(int threadId)
        {
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread == null) throw new KeyNotFoundException($"Thread {threadId} not found");
            thread.IsReported = false;
            await _context.SaveChangesAsync();
        }

        public async Task ClearPostReportAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) throw new KeyNotFoundException($"Post {postId} not found");
            post.IsReported = false;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteThreadAsync(int threadId)
        {
            var thread = await _context.Threads.Include(t => t.Posts).FirstOrDefaultAsync(t => t.ThreadId == threadId);
            if (thread == null) throw new KeyNotFoundException($"Thread {threadId} not found");
            _context.Posts.RemoveRange(thread.Posts);
            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) throw new KeyNotFoundException($"Post {postId} not found");
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task ReportThreadAsync(int threadId)
        {
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread == null) throw new KeyNotFoundException($"Thread {threadId} not found");
            thread.IsReported = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ForumThread>> FindThreadsAsync(ThreadFilter filter)
        {
            var query = _context.Threads
                .Include(t => t.Board)
                .Include(t => t.CreatedByNavigation)
                .AsQueryable();

            if (filter.ThreadId.HasValue)
                query = query.Where(t => t.ThreadId == filter.ThreadId.Value);
            if (filter.BoardId.HasValue)
                query = query.Where(t => t.BoardId == filter.BoardId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(t => t.Title.Contains(filter.Title));
            if (!string.IsNullOrWhiteSpace(filter.Message))
                query = query.Where(t => t.Message.Contains(filter.Message));
            if (!string.IsNullOrWhiteSpace(filter.Author))
                query = query.Where(t => t.CreatedByNavigation != null && t.CreatedByNavigation.Username.Contains(filter.Author));
            if (filter.IsReported.HasValue)
                query = query.Where(t => t.IsReported == filter.IsReported.Value);
            if (filter.CreatedFrom.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Post>> FindPostsAsync(PostFilter filter)
        {
            var query = _context.Posts
                .Include(p => p.Thread)
                .ThenInclude(t => t.Board)
                .Include(p => p.CreatedByNavigation)
                .AsQueryable();

            if (filter.PostId.HasValue)
                query = query.Where(p => p.PostId == filter.PostId.Value);
            if (filter.ThreadId.HasValue)
                query = query.Where(p => p.ThreadId == filter.ThreadId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Message))
                query = query.Where(p => p.Message.Contains(filter.Message));
            if (!string.IsNullOrWhiteSpace(filter.Author))
                query = query.Where(p => p.CreatedByNavigation != null && p.CreatedByNavigation.Username.Contains(filter.Author));
            if (filter.IsReported.HasValue)
                query = query.Where(p => p.IsReported == filter.IsReported.Value);
            if (filter.CreatedFrom.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedTo.Value);

            return await query.ToListAsync();
        }
    }
} 