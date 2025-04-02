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
            // Загружаем все треды с их изображениями
            Threads = await _context.Treads
                .Include(t => t.Images) // Подгружаем связанные изображения
                .ToListAsync();
        }
    }
}