using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace NewImageBoard.Pages
{
    public class CreateThreadModel : PageModel
    {
        private readonly ImageBoardContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateThreadModel(ImageBoardContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public class ThreadInputModel
        {
            [BindProperty]
            public string Title { get; set; }

            [BindProperty]
            public string Message { get; set; }

            [BindProperty]
            public IFormFile Image { get; set; } // Исправлено: удалено [BindNever]
        }

        [BindProperty]
        public ThreadInputModel NewThreadInput { get; set; }

        public Board Board { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int boardId)
        {
            Board = await _context.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);
            if (Board == null)
            {
                return NotFound();
            }

            NewThreadInput = new ThreadInputModel();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int boardId)
        {
            Board = await _context.Boards.FirstOrDefaultAsync(b => b.BoardId == boardId);
            if (Board == null)
            {
                return NotFound();
            }

            // Удаляем Image из ModelState
            ModelState.Remove("NewThreadInput.Image");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewThreadInput.Title) || string.IsNullOrWhiteSpace(NewThreadInput.Message))
            {
                ErrorMessage = "Title and message are required.";
                return Page();
            }

            var newThread = new ForumThread
            {
                BoardId = boardId,
                Title = NewThreadInput.Title,
                Message = NewThreadInput.Message,
                CreatedAt = DateTime.Now
            };

            // CreatedBy
            if (User.Identity.IsAuthenticated && User.Identity.Name != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                newThread.CreatedBy = user?.UserId;
            }
            else
            {
                newThread.CreatedBy = null;
            }

            // Обработка изображения
            if (NewThreadInput.Image != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + NewThreadInput.Image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await NewThreadInput.Image.CopyToAsync(fileStream);
                }

                newThread.ImagePath = "/images/" + uniqueFileName;
            }

            _context.Threads.Add(newThread);
            await _context.SaveChangesAsync();

            return RedirectToPage("/ThreadsView", new { boardId = boardId });
        }
    }
}
