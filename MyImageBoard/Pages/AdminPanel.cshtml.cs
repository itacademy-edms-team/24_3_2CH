using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace MyImageBoard.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IBoardService _boardService;
        private readonly ILogger<AdminPanelModel> _logger;

        public AdminPanelModel(
            IUserService userService,
            IBoardService boardService,
            ILogger<AdminPanelModel> logger)
        {
            _userService = userService;
            _boardService = boardService;
            _logger = logger;
        }

        [BindProperty]
        public string BoardName { get; set; }

        [BindProperty]
        public string BoardShortName { get; set; }

        [BindProperty]
        public string BoardDescription { get; set; }

        [BindProperty]
        public string ActionUserType { get; set; }

        [BindProperty]
        public string ActionUsername { get; set; }

        [BindProperty]
        public string ActionPassword { get; set; }

        public IList<User> Moderators { get; set; }
        public IList<User> Admins { get; set; }

        public string BoardErrorMessage { get; set; }
        public string ModeratorErrorMessage { get; set; }
        public string AdminErrorMessage { get; set; }
        public string RemoveModeratorErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                await LoadUserLists();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin panel");
                throw;
            }
        }

        public async Task<IActionResult> OnPostCreateBoardAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BoardName) || string.IsNullOrWhiteSpace(BoardShortName) || string.IsNullOrWhiteSpace(BoardDescription))
                {
                    BoardErrorMessage = "All fields are required.";
                    await LoadUserLists();
                    return Page();
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var board = new Board
                {
                    Name = BoardName,
                    ShortName = BoardShortName,
                    Description = BoardDescription
                };

                await _boardService.CreateBoardAsync(board, userId);
                await LoadUserLists();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating board");
                BoardErrorMessage = "An error occurred while creating the board.";
                await LoadUserLists();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostManageUserAsync()
        {
            try
            {
                await LoadUserLists();

                if (string.IsNullOrWhiteSpace(ActionUserType))
                {
                    RemoveModeratorErrorMessage = "Action type is missing.";
                    return Page();
                }

                switch (ActionUserType)
                {
                    case "AddModerator":
                        if (string.IsNullOrWhiteSpace(ActionUsername) || string.IsNullOrWhiteSpace(ActionPassword))
                        {
                            ModeratorErrorMessage = "Username and password are required.";
                            return Page();
                        }

                        var moderator = new User
                        {
                            Username = ActionUsername
                        };

                        await _userService.CreateUserAsync(moderator, ActionPassword);
                        break;

                    case "AddAdmin":
                        if (string.IsNullOrWhiteSpace(ActionUsername) || string.IsNullOrWhiteSpace(ActionPassword))
                        {
                            AdminErrorMessage = "Username and password are required.";
                            return Page();
                        }

                        var admin = new User
                        {
                            Username = ActionUsername
                        };

                        await _userService.CreateUserAsync(admin, ActionPassword);
                        break;

                    case "RemoveModerator":
                        if (string.IsNullOrWhiteSpace(ActionUsername))
                        {
                            RemoveModeratorErrorMessage = "Username is required.";
                            return Page();
                        }

                        var userToRemove = await _userService.GetUserByUsernameAsync(ActionUsername);
                        if (userToRemove == null || !await _userService.IsUserInGroupAsync(userToRemove.UserId, "Moderator"))
                        {
                            RemoveModeratorErrorMessage = "Moderator not found.";
                            return Page();
                        }

                        await _userService.DeleteUserAsync(userToRemove.UserId);
                        break;

                    default:
                        RemoveModeratorErrorMessage = "Invalid action type.";
                        return Page();
                }

                await LoadUserLists();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing users");
                RemoveModeratorErrorMessage = "An error occurred while managing users.";
                await LoadUserLists();
                return Page();
            }
        }

        private async Task LoadUserLists()
        {
            try
            {
                var allUsers = await _userService.GetAllUsersAsync();
                Moderators = allUsers.Where(u => u.Group?.Name == "Moderator").ToList();
                Admins = allUsers.Where(u => u.Group?.Name == "Admin").ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user lists");
                throw;
            }
        }
    }
}