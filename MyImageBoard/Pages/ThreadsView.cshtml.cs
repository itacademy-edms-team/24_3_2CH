using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NewImageBoard.Pages
{
    public class ThreadsViewModel : PageModel
    {
        private readonly ImageBoardContext _context;

        public ThreadsViewModel(ImageBoardContext context)
        {
            _context = context;
        }

        public Board Board { get; set; }

        public IList<ThreadViewModel> Threads { get; set; } = new List<ThreadViewModel>();

        public class ThreadViewModel
        {
            public int ThreadId { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string ImagePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; }
            public int ReplyCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int boardId)
        {
            Board = await _context.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);
            if (Board == null)
            {
                return NotFound();
            }

            Threads = await _context.Threads
                .Where(t => t.BoardId == boardId)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.Posts) // Подключаем посты (ответы) для подсчёта
                .Select(t => new ThreadViewModel
                {
                    ThreadId = t.ThreadId,
                    Title = t.Title,
                    Message = t.Message,
                    ImagePath = t.ImagePath,
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedByNavigation != null ? t.CreatedByNavigation.Username : "Anonymous",
                    ReplyCount = t.Posts.Count // Считаем количество постов (ответов)
                })
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}