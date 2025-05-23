using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard.Models;
using MyImageBoard.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyImageBoard.Pages
{
    public class BoardsViewModel : PageModel
    {
        private readonly IBoardService _boardService;
        private readonly IUserService _userService;

        public BoardsViewModel(IBoardService boardService, IUserService userService)
        {
            _boardService = boardService;
            _userService = userService;
        }

        public IList<BoardViewModel> Boards { get; set; } = new List<BoardViewModel>();
        public bool IsAdmin { get; set; }

        public class BoardViewModel
        {
            public int BoardId { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedByUsername { get; set; }
        }

        public async Task OnGetAsync()
        {
            var boards = await _boardService.GetAllBoardsAsync();
            var userId = User.Identity?.IsAuthenticated == true ? 
                int.Parse(User.FindFirst("UserId")?.Value ?? "0") : 0;

            IsAdmin = userId > 0 && await _userService.HasPermissionAsync(userId, "Admin");

            Boards = boards.Select(b => new BoardViewModel
            {
                BoardId = b.BoardId,
                Name = b.Name,
                ShortName = b.ShortName,
                Description = b.Description,
                CreatedAt = b.CreatedAt,
                CreatedByUsername = b.CreatedByNavigation?.Username ?? "Anonymous"
            }).ToList();
        }
    }
}