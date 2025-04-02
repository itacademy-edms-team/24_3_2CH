using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyImageBoard;

namespace MyImageBoard.Pages.ThreadPages
{
    public class ThreadsModel : PageModel
    {
        private readonly MyImageBoardContext _context;

        public ThreadsModel(MyImageBoardContext context)
        {
            _context = context;
        }

        public List<Tread> Threads { get; set; }

        public async Task OnGetAsync()
        {
            // ��������� ��� ����� � �� �������������
            Threads = await _context.Treads
                .Include(t => t.Images) // ���������� ��������� �����������
                .ToListAsync();
        }
    }
}