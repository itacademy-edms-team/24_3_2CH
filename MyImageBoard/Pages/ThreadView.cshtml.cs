using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MyImageBoard.Pages
{
    public class ThreadViewModel : PageModel
    {
        private readonly IThreadService _threadService;
        private readonly IBoardService _boardService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;

        public ThreadViewModel(
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

        public ForumThread Thread { get; set; }
        public Board Board { get; set; }
        public IList<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public bool IsModerator { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int boardId, int threadId, int page = 1)
        {
            try
            {
                Board = await _boardService.GetBoardByIdAsync(boardId);
                if (Board == null)
                {
                    return NotFound();
                }

                Thread = await _threadService.GetThreadByIdAsync(threadId);
                if (Thread == null || Thread.BoardId != boardId)
                {
                    return NotFound();
                }

                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

                IsModerator = userId > 0 && await _userService.HasPermissionAsync(userId, "ModeratePost");

                CurrentPage = page;
                var posts = await _threadService.GetThreadPostsAsync(threadId, page, PageSize);
                var totalPosts = await _threadService.GetThreadPostsCountAsync(threadId);
                TotalPages = (int)Math.Ceiling(totalPosts / (double)PageSize);

                Posts = posts.Select(p => new PostViewModel
                {
                    PostId = p.PostId,
                    Message = p.Message,
                    ImagePath = p.ImagePath,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedByNavigation?.Username ?? "Anonymous",
                    IsReported = p.IsReported
                }).ToList();

                NewPostInput = new PostInputModel();
                return Page();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while loading the thread");
            }
        }

        public async Task<IActionResult> OnPostAsync(int boardId, int threadId)
        {
            try
            {
                Board = await _boardService.GetBoardByIdAsync(boardId);
                if (Board == null)
                {
                    return NotFound();
                }

                Thread = await _threadService.GetThreadByIdAsync(threadId);
                if (Thread == null || Thread.BoardId != boardId)
                {
                    return NotFound();
                }

                // Удаляем Image из ModelState
                ModelState.Remove("NewPostInput.Image");

                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(NewPostInput.Message))
                {
                    ErrorMessage = "Please enter a message.";
                    NewPostInput.Image = null; // сбрасываем картинку, чтобы не ломать разметку
                    return Page();
                }

                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

                if (!await _postService.ValidatePostContentAsync(NewPostInput.Message))
                {
                    ErrorMessage = "Invalid message content.";
                    return Page();
                }

                if (NewPostInput.Image != null && !await _postService.ValidateImageAsync(NewPostInput.Image))
                {
                    ErrorMessage = "Invalid image file.";
                    return Page();
                }

                var newPost = new Post
                {
                    ThreadId = threadId,
                    Message = NewPostInput.Message,
                    CreatedBy = userId
                };

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

                await _postService.CreatePostAsync(newPost, userId);

                return RedirectToPage("/ThreadView", new { boardId, threadId });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while creating the post");
            }
        }

        public async Task<IActionResult> OnPostReportThreadAsync(int boardId, int threadId)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

                var thread = await _threadService.GetThreadByIdAsync(threadId);
                if (thread == null || thread.BoardId != boardId)
                {
                    return NotFound();
                }

                // Здесь можно добавить логику репортов, если она нужна
                // Например, создание записи в таблице репортов

                return RedirectToPage("/ThreadView", new { boardId, threadId });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while reporting the thread");
            }
        }

        public async Task<IActionResult> OnPostReportPostAsync(int boardId, int threadId, int postId)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? 
                    int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

                var post = await _postService.GetPostByIdAsync(postId);
                if (post == null || post.ThreadId != threadId)
                {
                    return NotFound();
                }

                // Здесь можно добавить логику репортов, если она нужна
                // Например, создание записи в таблице репортов

                return RedirectToPage("/ThreadView", new { boardId, threadId });
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                return StatusCode(500, "An error occurred while reporting the post");
            }
        }
    }
}