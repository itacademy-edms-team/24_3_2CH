using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyImageBoard; // ����� ���������� ������������ ���� ��� �������

namespace MyImageBoard.Pages.ThreadPages
{
    public class NewThreadModel : PageModel
    {
        private readonly MyImageBoardContext _context;

        public NewThreadModel(MyImageBoardContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tread Thread { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            // ������ ����� ��� ����������� �����
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Treads.Add(Thread);
            await _context.SaveChangesAsync();

            Message = "Thread created successfully!";
            Thread = new Tread(); // ������� ����� ����� ����������

            return Page();
        }
    }
}