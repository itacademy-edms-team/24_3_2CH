using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewImageBoard.Pages
{
    [Authorize(Roles = "Moderator,Admin")]
    public class ModeratorPanelModel : PageModel
    {
        private readonly ImageBoardContext _context;

        public ModeratorPanelModel(ImageBoardContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string ThreadSearchText { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ThreadIsReported { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ThreadBoardId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ThreadSortOrder { get; set; } = "CreatedAtDesc";

        [BindProperty(SupportsGet = true)]
        public int? PostThreadId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PostSearchText { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool PostIsReported { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PostBoardId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PostSortOrder { get; set; } = "CreatedAtDesc";

        [BindProperty(SupportsGet = true)]
        public int? ThreadId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PostId { get; set; }

        public IList<ForumThread> ForumThreads { get; set; } = new List<ForumThread>();
        public IList<Post> Posts { get; set; } = new List<Post>();
        public IList<Board> Boards { get; set; } = new List<Board>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Загрузка списка досок
            Boards = await _context.Boards.OrderBy(b => b.ShortName).ToListAsync();

            // Фильтрация тредов
            var threadsQuery = _context.Threads
                .Include(t => t.Board)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(ThreadSearchText))
            {
                threadsQuery = threadsQuery.Where(t => t.Title.Contains(ThreadSearchText) || t.Message.Contains(ThreadSearchText));
            }

            if (ThreadIsReported)
            {
                threadsQuery = threadsQuery.Where(t => t.IsReported);
            }

            if (ThreadBoardId.HasValue)
            {
                threadsQuery = threadsQuery.Where(t => t.BoardId == ThreadBoardId.Value);
            }

            if (ThreadId.HasValue)
            {
                threadsQuery = threadsQuery.Where(t => t.ThreadId == ThreadId.Value);
            }

            // Сортировка тредов
            threadsQuery = ThreadSortOrder switch
            {
                "CreatedAtAsc" => threadsQuery.OrderBy(t => t.CreatedAt),
                "TitleAsc" => threadsQuery.OrderBy(t => t.Title),
                "TitleDesc" => threadsQuery.OrderByDescending(t => t.Title),
                _ => threadsQuery.OrderByDescending(t => t.CreatedAt),
            };

            ForumThreads = await threadsQuery.ToListAsync();

            // Фильтрация постов
            var postsQuery = _context.Posts
                .Include(p => p.Thread)
                .ThenInclude(t => t.Board)
                .AsQueryable();

            if (PostThreadId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.ThreadId == PostThreadId.Value);
            }

            if (!string.IsNullOrWhiteSpace(PostSearchText))
            {
                postsQuery = postsQuery.Where(p => p.Message.Contains(PostSearchText));
            }

            if (PostIsReported)
            {
                postsQuery = postsQuery.Where(p => p.IsReported);
            }

            if (PostBoardId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.Thread.BoardId == PostBoardId.Value);
            }

            if (PostId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.PostId == PostId.Value);
            }

            // Сортировка постов
            postsQuery = PostSortOrder switch
            {
                "CreatedAtAsc" => postsQuery.OrderBy(p => p.CreatedAt),
                _ => postsQuery.OrderByDescending(p => p.CreatedAt),
            };

            Posts = await postsQuery.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostClearReportThreadAsync(int threadId)
        {
            var thread = await _context.Threads.FirstOrDefaultAsync(t => t.ThreadId == threadId);
            if (thread == null)
            {
                return NotFound();
            }

            thread.IsReported = false;
            await _context.SaveChangesAsync();

            return RedirectToPage("/ModeratorPanel", new { ThreadSearchText, ThreadIsReported, ThreadBoardId, ThreadSortOrder, PostThreadId, PostSearchText, PostIsReported, PostBoardId, PostSortOrder });
        }

        public async Task<IActionResult> OnPostClearReportPostAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null)
            {
                return NotFound();
            }

            post.IsReported = false;
            await _context.SaveChangesAsync();

            return RedirectToPage("/ModeratorPanel", new { ThreadSearchText, ThreadIsReported, ThreadBoardId, ThreadSortOrder, PostThreadId, PostSearchText, PostIsReported, PostBoardId, PostSortOrder });
        }

        public async Task<IActionResult> OnPostDeleteThreadAsync(int threadId)
        {
            var thread = await _context.Threads.FirstOrDefaultAsync(t => t.ThreadId == threadId);
            if (thread == null)
            {
                return NotFound();
            }

            _context.Threads.Remove(thread);
            await _context.SaveChangesAsync();

            return RedirectToPage("/ModeratorPanel", new { ThreadSearchText, ThreadIsReported, ThreadBoardId, ThreadSortOrder, PostThreadId, PostSearchText, PostIsReported, PostBoardId, PostSortOrder });
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToPage("/ModeratorPanel", new { ThreadSearchText, ThreadIsReported, ThreadBoardId, ThreadSortOrder, PostThreadId, PostSearchText, PostIsReported, PostBoardId, PostSortOrder });
        }
    }
}