using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyImageBoard.Services;
using MyImageBoard.Services.Interfaces;

namespace MyImageBoard.Pages
{
    [Authorize(Roles = "Moderator,Admin")]
    public class ModeratorPanelModel : PageModel
    {
        private readonly IModerationService _moderationService;

        public ModeratorPanelModel(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        // Фильтры для поиска тредов
        [BindProperty(SupportsGet = true)] public int? ThreadId { get; set; }
        [BindProperty(SupportsGet = true)] public int? ThreadBoardId { get; set; }
        [BindProperty(SupportsGet = true)] public string ThreadTitle { get; set; }
        [BindProperty(SupportsGet = true)] public string ThreadMessage { get; set; }
        [BindProperty(SupportsGet = true)] public string ThreadAuthor { get; set; }
        [BindProperty(SupportsGet = true)] public bool? ThreadIsReported { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ThreadCreatedFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ThreadCreatedTo { get; set; }

        // Фильтры для поиска постов
        [BindProperty(SupportsGet = true)] public int? PostId { get; set; }
        [BindProperty(SupportsGet = true)] public int? PostThreadId { get; set; }
        [BindProperty(SupportsGet = true)] public string PostMessage { get; set; }
        [BindProperty(SupportsGet = true)] public string PostAuthor { get; set; }
        [BindProperty(SupportsGet = true)] public bool? PostIsReported { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? PostCreatedFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? PostCreatedTo { get; set; }

        public IList<ForumThread> ForumThreads { get; set; } = new List<ForumThread>();
        public IList<Post> Posts { get; set; } = new List<Post>();
        public IList<Board> Boards { get; set; } = new List<Board>();

        public async Task<IActionResult> OnGetAsync()
        {
            var threadFilter = new ThreadFilter
            {
                ThreadId = ThreadId,
                BoardId = ThreadBoardId,
                Title = ThreadTitle,
                Message = ThreadMessage,
                Author = ThreadAuthor,
                IsReported = ThreadIsReported,
                CreatedFrom = ThreadCreatedFrom,
                CreatedTo = ThreadCreatedTo
            };
            var postFilter = new PostFilter
            {
                PostId = PostId,
                ThreadId = PostThreadId,
                Message = PostMessage,
                Author = PostAuthor,
                IsReported = PostIsReported,
                CreatedFrom = PostCreatedFrom,
                CreatedTo = PostCreatedTo
            };
            ForumThreads = (await _moderationService.FindThreadsAsync(threadFilter)).ToList();
            Posts = (await _moderationService.FindPostsAsync(postFilter)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostClearReportThreadAsync(int threadId)
        {
            await _moderationService.ClearThreadReportAsync(threadId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearReportPostAsync(int postId)
        {
            await _moderationService.ClearPostReportAsync(postId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteThreadAsync(int threadId)
        {
            await _moderationService.DeleteThreadAsync(threadId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            await _moderationService.DeletePostAsync(postId);
            return RedirectToPage();
        }
    }
}