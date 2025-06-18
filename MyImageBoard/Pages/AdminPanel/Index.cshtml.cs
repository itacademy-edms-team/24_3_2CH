using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ForumProject.Pages.AdminPanel
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISuperUserService _superUserService;
        private readonly ICommentService _commentService;
        private readonly IThreadService _threadService;
        private readonly IBoardService _boardService;

        public SuperUser? CurrentSuperUser { get; set; }
        public IEnumerable<SuperUser> SuperUsers { get; set; } = Enumerable.Empty<SuperUser>();
        public IEnumerable<SiteThread> Threads { get; set; } = Enumerable.Empty<SiteThread>();
        public IEnumerable<Comment> Comments { get; set; } = Enumerable.Empty<Comment>();
        public IEnumerable<Complaint> Complaints { get; set; } = Enumerable.Empty<Complaint>();
        public IEnumerable<Board> Boards { get; set; } = Enumerable.Empty<Board>();
        public IEnumerable<Permission> UserPermissions { get; set; } = Enumerable.Empty<Permission>();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public IndexModel(
            ApplicationDbContext context, 
            ISuperUserService superUserService, 
            ICommentService commentService,
            IThreadService threadService,
            IBoardService boardService)
        {
            _context = context;
            _superUserService = superUserService;
            _commentService = commentService;
            _threadService = threadService;
            _boardService = boardService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var superUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (superUserId.HasValue)
            {
                CurrentSuperUser = await _superUserService.GetSuperUserByIdAsync(superUserId.Value);
                if (CurrentSuperUser?.IsBlocked == true)
                {
                    HttpContext.Session.Remove("SuperUserId");
                    ErrorMessage = "Your account has been blocked.";
                    return Page();
                }

                UserPermissions = await _superUserService.GetUserPermissionsAsync(superUserId.Value);
                await LoadPageData();
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and password are required.";
                return Page();
            }

            var (success, message, user) = await _superUserService.AuthenticateAsync(Username, Password);
            if (!success || user == null)
            {
                ErrorMessage = message;
                return Page();
            }

            HttpContext.Session.SetInt32("SuperUserId", user.Id);
            CurrentSuperUser = user;
            UserPermissions = await _superUserService.GetUserPermissionsAsync(user.Id);
            await LoadPageData();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateSuperUserAsync(string newUsername, string newPassword, int groupId, List<string> permissions)
        {
            var currentUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!currentUserId.HasValue || !await _superUserService.HasPermissionAsync(currentUserId.Value, "CreateSuperUser"))
                return Forbid();

            var (success, message, _) = await _superUserService.CreateSuperUserAsync(
                newUsername,
                newPassword,
                groupId,
                permissions,
                currentUserId.Value);

            if (!success)
            {
                ErrorMessage = message;
                return RedirectToPage();
            }

            SuccessMessage = message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleBlockAsync(int userId)
        {
            var currentUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!currentUserId.HasValue)
                return Forbid();

            var (success, message) = await _superUserService.ToggleBlockStatusAsync(userId, currentUserId.Value);
            
            if (!success)
            {
                ErrorMessage = message;
                return RedirectToPage();
            }

            SuccessMessage = message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteThreadAsync(int threadId)
        {
            var currentUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!currentUserId.HasValue || !await _superUserService.HasPermissionAsync(currentUserId.Value, "DeleteThread"))
                return Forbid();

            try
            {
                var success = await _threadService.DeleteThreadAsync(threadId);
                if (!success)
                {
                    ErrorMessage = $"Thread with ID {threadId} not found or could not be deleted.";
                    return RedirectToPage();
                }

                SuccessMessage = $"Thread with ID {threadId} deleted successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting thread {threadId}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Admin panel error deleting thread {threadId}: {ex}");
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var currentUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!currentUserId.HasValue || !await _superUserService.HasPermissionAsync(currentUserId.Value, "DeleteComment"))
                return Forbid();

            try
            {
                var success = await _commentService.DeleteCommentAsync(commentId);
                if (!success)
                {
                    ErrorMessage = $"Comment with ID {commentId} not found or could not be deleted.";
                    return RedirectToPage();
                }

                SuccessMessage = $"Comment with ID {commentId} deleted successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting comment {commentId}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Admin panel error deleting comment {commentId}: {ex}");
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteComplaintAsync(int complaintId)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null)
            {
                return NotFound();
            }

            _context.Complaints.Remove(complaint);
            await _context.SaveChangesAsync();

            SuccessMessage = "Complaint deleted successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateBoardAsync(string title, string description)
        {
            var superUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!superUserId.HasValue)
            {
                return RedirectToPage();
            }

            CurrentSuperUser = await _superUserService.GetSuperUserByIdAsync(superUserId.Value);
            if (CurrentSuperUser == null)
            {
                return RedirectToPage();
            }

            if (!await _superUserService.HasPermissionAsync(CurrentSuperUser.Id, "CreateBoard"))
            {
                ErrorMessage = "You don't have permission to create boards.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Board title is required.";
                return RedirectToPage();
            }

            var board = new Board
            {
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _boardService.CreateBoardAsync(board);
            SuccessMessage = "Board created successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteBoardAsync(int boardId)
        {
            var superUserId = HttpContext.Session.GetInt32("SuperUserId");
            if (!superUserId.HasValue)
            {
                return RedirectToPage();
            }

            CurrentSuperUser = await _superUserService.GetSuperUserByIdAsync(superUserId.Value);
            if (CurrentSuperUser == null)
            {
                return RedirectToPage();
            }

            if (!await _superUserService.HasPermissionAsync(CurrentSuperUser.Id, "DeleteBoard"))
            {
                ErrorMessage = "You don't have permission to delete boards.";
                return RedirectToPage();
            }

            var success = await _boardService.DeleteBoardAsync(boardId);
            if (!success)
            {
                ErrorMessage = "Failed to delete board. It might not exist or there was an error.";
                return RedirectToPage();
            }

            SuccessMessage = "Board deleted successfully.";
            return RedirectToPage();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("SuperUserId");
            return RedirectToPage();
        }

        private async Task LoadPageData()
        {
            if (CurrentSuperUser == null) return;

            SuperUsers = await _superUserService.GetAllSuperUsersAsync();
            UserPermissions = await _superUserService.GetUserPermissionsAsync(CurrentSuperUser.Id);

            Threads = await _context.Threads
                .Include(t => t.Board)
                .OrderByDescending(t => t.CreatedAt)
                .Take(200)
                .ToListAsync();

            Comments = await _context.Comments
                .Include(c => c.Thread)
                .OrderByDescending(c => c.CreatedAt)
                .Take(200)
                .ToListAsync();

            Complaints = await _context.Complaints
                .Include(c => c.Thread)
                .Include(c => c.Comment)
                .OrderByDescending(c => c.CreatedAt)
                .Take(200)
                .ToListAsync();

            Boards = await _context.Boards
                .OrderBy(b => b.Title)
                .ToListAsync();
        }
    }
} 