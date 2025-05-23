using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MyImageBoard.Pages
{
    public class ThreadsViewModel : PageModel
    {
        private readonly IThreadService _threadService;
        private readonly IBoardService _boardService;
        private readonly IUserService _userService;
        private readonly IModerationService _moderationService;

        public ThreadsViewModel(
            IThreadService threadService,
            IBoardService boardService,
            IUserService userService,
            IModerationService moderationService)
        {
            _threadService = threadService;
            _boardService = boardService;
            _userService = userService;
            _moderationService = moderationService;
        }

        public Board Board { get; set; }
        public IList<ThreadViewModel> Threads { get; set; } = new List<ThreadViewModel>();
        public bool IsModerator { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public class ThreadViewModel
        {
            public int ThreadId { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string ImagePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; }
            public int ReplyCount { get; set; }
            public bool IsReported { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int boardId, int page = 1)
        {
            try
            {
                Board = await _boardService.GetBoardByIdAsync(boardId);
                if (Board == null)
                {
                    return NotFound();
                }

                CurrentPage = page;
                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

                IsModerator = userId > 0 && await _userService.HasPermissionAsync(userId, "ModerateThread");

                var threads = await _threadService.GetThreadsByBoardAsync(boardId, page, PageSize);
                var totalPosts = await _threadService.GetThreadPostsCountAsync(boardId);
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PageSize);

                Threads = threads.Select(t => new ThreadViewModel
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Message = t.Message,
                    ImagePath = t.ImagePath,
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedByNavigation?.Username ?? "Anonymous",
                    ReplyCount = t.Posts?.Count ?? 0,
                    IsReported = t.IsReported
                }).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while loading threads");
            }
        }

        public async Task<IActionResult> OnPostReportThreadAsync(int threadId)
        {
            try
            {
                await _moderationService.ReportThreadAsync(threadId);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while reporting the thread");
            }
        }
    }
}