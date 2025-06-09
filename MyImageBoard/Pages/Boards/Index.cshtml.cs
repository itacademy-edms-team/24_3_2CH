using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc; // ������ ���� using ��� IActionResult

namespace ForumProject.Pages.Boards
{
    public class IndexModel : PageModel
    {
        private readonly IBoardService _boardService;

        // ����������� IBoardService ����� �����������
        public IndexModel(IBoardService boardService)
        {
            _boardService = boardService;
        }

        public IEnumerable<Board> Boards { get; set; } = Enumerable.Empty<Board>(); // �������������� ������ ����������

        // �����, ������� ����������� ��� HTTP GET ������� � ��������
        public async Task OnGetAsync()
        {
            Boards = await _boardService.GetAllBoardsAsync();
        }
    }
}