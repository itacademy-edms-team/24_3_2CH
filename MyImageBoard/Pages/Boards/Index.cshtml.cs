using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc; // Добавь этот using для IActionResult

namespace ForumProject.Pages.Boards
{
    public class IndexModel : PageModel
    {
        private readonly IBoardService _boardService;

        // Инжектируем IBoardService через конструктор
        public IndexModel(IBoardService boardService)
        {
            _boardService = boardService;
        }

        public IEnumerable<Board> Boards { get; set; } = Enumerable.Empty<Board>(); // Инициализируем пустой коллекцией

        // Метод, который выполняется при HTTP GET запросе к странице
        public async Task OnGetAsync()
        {
            Boards = await _boardService.GetAllBoardsAsync();
        }
    }
}