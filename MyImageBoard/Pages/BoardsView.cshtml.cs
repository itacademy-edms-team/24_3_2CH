using Microsoft.AspNetCore.Mvc.RazorPages;
using NewImageBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewImageBoard.Pages
{
    public class BoardsViewModel : PageModel
    {
        private readonly ImageBoardContext _context;

        public BoardsViewModel(ImageBoardContext context)
        {
            _context = context;
        }

        public IList<BoardViewModel> Boards { get; set; } = new List<BoardViewModel>();

        public class BoardViewModel
        {
            public int BoardId { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public async Task OnGetAsync()
        {
            Boards = await _context.Boards
                .Select(b => new BoardViewModel
                {
                    BoardId = b.BoardId,
                    Name = b.Name,
                    ShortName = b.ShortName,
                    Description = b.Description,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }
    }
}