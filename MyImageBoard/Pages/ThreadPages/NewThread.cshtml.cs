using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MyImageBoard.Pages.ThreadPages
{
    public class NewThreadModel : PageModel
    {
        private readonly MyImageBoardContext _context;
        private readonly IWebHostEnvironment _environment;

        public NewThreadModel(MyImageBoardContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public InputModel ThreadInput { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public string ErrorMessage { get; set; } // ��������� ������ ErrorMessage

        public class InputModel
        {
            [Required(ErrorMessage = "Title is required.")]
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public void OnGet()
        {
            ThreadInput = new InputModel();
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ��������� ������ Title ����
            if (string.IsNullOrEmpty(ThreadInput.Title))
            {
                TempData["ErrorMessage"] = "Title is required.";
                return RedirectToPage("./NewThread");
            }

            // ������ ������ Tread �� ThreadInput
            var thread = new Tread
            {
                Title = ThreadInput.Title,
                Text = ThreadInput.Text,
                Images = new List<Image>()
            };

            // ������������ �����������
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                thread.Images.Add(new Image
                {
                    ImageUrl = "/images/" + uniqueFileName
                });
            }

            // ��������� ����
            _context.Treads.Add(thread);
            await _context.SaveChangesAsync();

            // ������� TempData["Message"]
            return RedirectToPage("./Threads");
        }
    }
}