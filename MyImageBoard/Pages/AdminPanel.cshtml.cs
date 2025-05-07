using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Collections.Generic;
using System.Security.Claims;

namespace NewImageBoard.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly ImageBoardContext _context;

        public AdminPanelModel(ImageBoardContext context)
        {
            _context = context;
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
            await LoadUserLists();
        }

        public async Task<IActionResult> OnPostCreateBoardAsync()
        {
            if (string.IsNullOrWhiteSpace(BoardName) || string.IsNullOrWhiteSpace(BoardShortName) || string.IsNullOrWhiteSpace(BoardDescription))
            {
                BoardErrorMessage = "All fields are required.";
                await LoadUserLists();
                return Page();
            }

            if (await _context.Boards.AnyAsync(b => b.Name == BoardName || b.ShortName == BoardShortName))
            {
                BoardErrorMessage = "Board name or short name already exists.";
                await LoadUserLists();
                return Page();
            }

            var board = new Board
            {
                Name = BoardName,
                ShortName = BoardShortName,
                Description = BoardDescription,
                CreatedAt = DateTime.Now,
                CreatedBy = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();
            await LoadUserLists();
            return Page();
        }

        public async Task<IActionResult> OnPostManageUserAsync()
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
                    if (await _context.Users.AnyAsync(u => u.Username == ActionUsername))
                    {
                        ModeratorErrorMessage = "Username already exists.";
                        return Page();
                    }
                    var moderatorGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "Moderator");
                    if (moderatorGroup == null)
                    {
                        ModeratorErrorMessage = "Moderator group not found. Please ensure the 'Moderator' group exists in the database.";
                        return Page();
                    }
                    var moderator = new User
                    {
                        Username = ActionUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(ActionPassword),
                        GroupId = moderatorGroup.GroupId,
                        CreatedAt = DateTime.Now
                    };
                    _context.Users.Add(moderator);
                    break;

                case "AddAdmin":
                    if (string.IsNullOrWhiteSpace(ActionUsername) || string.IsNullOrWhiteSpace(ActionPassword))
                    {
                        AdminErrorMessage = "Username and password are required.";
                        return Page();
                    }
                    if (await _context.Users.AnyAsync(u => u.Username == ActionUsername))
                    {
                        AdminErrorMessage = "Username already exists.";
                        return Page();
                    }
                    var adminGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Name == "Admin");
                    if (adminGroup == null)
                    {
                        AdminErrorMessage = "Admin group not found. Please ensure the 'Admin' group exists in the database.";
                        return Page();
                    }
                    var admin = new User
                    {
                        Username = ActionUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(ActionPassword),
                        GroupId = adminGroup.GroupId,
                        CreatedAt = DateTime.Now
                    };
                    _context.Users.Add(admin);
                    break;

                case "RemoveModerator":
                    if (string.IsNullOrWhiteSpace(ActionUsername))
                    {
                        RemoveModeratorErrorMessage = "Username is required.";
                        return Page();
                    }
                    var removeModerator = await _context.Users
                        .Include(u => u.Group)
                        .FirstOrDefaultAsync(u => u.Username == ActionUsername && u.Group != null && u.Group.Name == "Moderator");
                    if (removeModerator == null)
                    {
                        RemoveModeratorErrorMessage = "Moderator not found.";
                        return Page();
                    }
                    _context.Users.Remove(removeModerator);
                    break;

                default:
                    RemoveModeratorErrorMessage = "Invalid action type.";
                    return Page();
            }

            await _context.SaveChangesAsync();
            await LoadUserLists();
            return Page();
        }

        private async Task LoadUserLists()
        {
            Moderators = await _context.Users
                .Include(u => u.Group)
                .Where(u => u.Group != null && u.Group.Name == "Moderator")
                .ToListAsync();

            Admins = await _context.Users
                .Include(u => u.Group)
                .Where(u => u.Group != null && u.Group.Name == "Admin")
                .ToListAsync();
        }
    }
}