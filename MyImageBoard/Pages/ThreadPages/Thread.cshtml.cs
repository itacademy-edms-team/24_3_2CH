using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyImageBoard;
using Microsoft.AspNetCore.Http;

namespace MyImageBoard.Pages.ThreadPages
{
    public class ThreadModel : PageModel
    {
        private readonly MyImageBoardContext _context;
        private readonly IWebHostEnvironment _environment;

        public ThreadModel(MyImageBoardContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public Tread Thread { get; set; }
        public List<Comment> Comments { get; set; }
        public string Message { get; set; }

        [BindProperty]
        public IFormFile CommentImage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Thread = await _context.Treads
                .Include(t => t.Images)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Images)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Thread == null)
            {
                return NotFound();
            }

            Comments = Thread.Comments?.ToList() ?? new List<Comment>();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, string CommentText)
        {
            // Загружаем только тред без комментариев для добавления нового
            Thread = await _context.Treads
                .Include(t => t.Images)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (Thread == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(CommentText))
            {
                var comment = new Comment { Text = CommentText };

                // Обработка изображения
                if (CommentImage != null && CommentImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(CommentImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await CommentImage.CopyToAsync(fileStream);
                    }

                    comment.Images = new List<Image>
                    {
                        new Image { ImageUrl = "/images/" + uniqueFileName }
                    };
                }

                // Добавляем комментарий в таблицу Comments
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Создаем запись в таблице Tread_Comment вручную
                _context.Database.ExecuteSqlRaw(
                    "INSERT INTO Tread_Comment (tread_id, comment_id) VALUES ({0}, {1})",
                    Thread.Id, comment.Id);

                Message = "Comment added successfully!";
            }

            // Перезагружаем тред с комментариями для отображения
            Thread = await _context.Treads
                .Include(t => t.Images)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Images)
                .FirstOrDefaultAsync(t => t.Id == id);

            Comments = Thread.Comments?.ToList() ?? new List<Comment>();
            return Page();
        }
    }
}