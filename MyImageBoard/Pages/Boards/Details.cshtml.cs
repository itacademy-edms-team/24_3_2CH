using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ForumProject.Pages.Boards
{
    public class DetailsModel : PageModel
    {
        private readonly IBoardService _boardService;

        public DetailsModel(IBoardService boardService)
        {
            _boardService = boardService;
        }

        public Board? Board { get; set; }

        // Метод, который вызывается при HTTP GET запросе к странице,
        // когда в URL есть параметр {id}
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Board = await _boardService.GetBoardByIdAsync(id);

            if (Board == null)
            {
                return NotFound(); // Возвращаем 404, если доска не найдена
            }

            return Page(); // Отображаем страницу
        }
    }
}