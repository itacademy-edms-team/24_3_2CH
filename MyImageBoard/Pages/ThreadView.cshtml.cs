using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using NewImageBoard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace NewImageBoard.Pages
{
    public class ThreadViewModel : PageModel
    {
        private readonly ImageBoardContext _context;
        private readonly IWebHostEnvironment _environment;

        public ThreadViewModel(ImageBoardContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public ForumThread Thread { get; set; }
        public Board Board { get; set; }
        public IList<PostViewModel> Posts { get; set; } = new List<PostViewModel>();

        public class PostViewModel
        {
            public int PostId { get; set; }
            public string Message { get; set; }
            public string ImagePath { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; }
            public bool IsReported { get; set; }
        }

        public class PostInputModel
        {
            public string Message { get; set; }
            public IFormFile Image { get; set; }
        }

        [BindProperty]
        public PostInputModel NewPostInput { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int boardId, int threadId)
        {
            Board = await _context.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);
            if (Board == null)
            {
                return NotFound();
            }

            Thread = await _context.Threads
            .Include(t => t.CreatedByNavigation)
            .FirstOrDefaultAsync(t => t.ThreadId == threadId && t.BoardId == boardId);
            if (Thread == null)
            {
                return NotFound();
            }

            Posts = await _context.Posts
            .Where(p => p.ThreadId == threadId)
            .Include(p => p.CreatedByNavigation)
            .Select(p => new PostViewModel
            {
                PostId = p.PostId,
                Message = p.Message,
                ImagePath = p.ImagePath,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedByNavigation != null && p.CreatedBy != null ? p.CreatedByNavigation.Username : "Аноним",
                IsReported = p.IsReported
            })
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

            NewPostInput = new PostInputModel();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int boardId, int threadId)
        {
            Board = await _context.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);
            if (Board == null)
            {
                return NotFound();
            }

            Thread = await _context.Threads.FirstOrDefaultAsync(t => t.ThreadId == threadId && t.BoardId == boardId);
            if (Thread == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(NewPostInput.Message))
            {
                ErrorMessage = "Сообщение обязательно.";
                return Page();
            }

            var newPost = new Post
            {
                ThreadId = threadId,
                Message = NewPostInput.Message,
                CreatedAt = DateTime.Now
            };

            if (User.Identity.IsAuthenticated && User.Identity.Name != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                newPost.CreatedBy = user?.UserId;
            }

            if (NewPostInput.Image != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + NewPostInput.Image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await NewPostInput.Image.CopyToAsync(fileStream);
                }

                newPost.ImagePath = "/images/" + uniqueFileName;
            }

            _context.Posts.Add(newPost);
            await _context.SaveChangesAsync();

            return RedirectToPage("/ThreadView", new { boardId, threadId });
        }

        public async Task<IActionResult> OnPostReportThreadAsync(int boardId, int threadId)
        {
            var thread = await _context.Threads.FirstOrDefaultAsync(t => t.ThreadId == threadId && t.BoardId == boardId);
            if (thread == null)
            {
                return NotFound();
            }

            thread.IsReported = true;
            await _context.SaveChangesAsync();

            return RedirectToPage("/ThreadView", new { boardId, threadId });
        }

        public async Task<IActionResult> OnPostReportPostAsync(int boardId, int threadId, int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId && p.ThreadId == threadId);
            if (post == null)
            {
                return NotFound();
            }

            post.IsReported = true;
            await _context.SaveChangesAsync();

            return RedirectToPage("/ThreadView", new { boardId, threadId });
        }
    }
}