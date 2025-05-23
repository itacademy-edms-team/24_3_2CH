using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;

namespace MyImageBoard.Pages
{
    public class CreateThreadModel : PageModel
    {
        private readonly IThreadService _threadService;
        private readonly IBoardService _boardService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;

        public CreateThreadModel(
            IThreadService threadService,
            IBoardService boardService,
            IPostService postService,
            IUserService userService,
            IWebHostEnvironment environment)
        {
            _threadService = threadService;
            _boardService = boardService;
            _postService = postService;
            _userService = userService;
            _environment = environment;
        }

        public class ThreadInputModel
        {
            [BindProperty]
            public string Title { get; set; }

            [BindProperty]
            public string Message { get; set; }

            [BindProperty]
            public IFormFile Image { get; set; }
        }

        [BindProperty]
        public ThreadInputModel NewThreadInput { get; set; }

        public Board Board { get; set; }
        public List<Board> Boards { get; set; } = new List<Board>();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int boardId)
        {
            try
            {
                Board = await _boardService.GetBoardByIdAsync(boardId);
                if (Board == null)
                {
                    return NotFound();
                }
                Boards = (List<Board>)await _boardService.GetAllBoardsAsync();
                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;
                NewThreadInput = new ThreadInputModel();
                return Page();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while loading the page");
            }
        }

        public async Task<IActionResult> OnPostAsync(int boardId)
        {
            try
            {
                Board = await _boardService.GetBoardByIdAsync(boardId);
                if (Board == null)
                {
                    return NotFound();
                }

                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

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

                if (!await _postService.ValidatePostContentAsync(NewThreadInput.Message))
                {
                    ErrorMessage = "Invalid message content.";
                    return Page();
                }

                if (NewThreadInput.Image != null && !await _postService.ValidateImageAsync(NewThreadInput.Image))
                {
                    ErrorMessage = "Invalid image file.";
                    return Page();
                }

                var newThread = new ForumThread
                {
                    BoardId = boardId,
                    Title = NewThreadInput.Title,
                    Message = NewThreadInput.Message,
                    CreatedBy = userId
                };

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

                await _threadService.CreateThreadAsync(newThread, userId);

                return RedirectToPage("/ThreadsView", new { boardId = boardId });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while creating the thread");
            }
        }
    }
}
