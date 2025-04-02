using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations; // Для атрибутов валидации

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

        // Добавляем атрибут валидации для Thread
        [BindProperty]
        public InputModel ThreadInput { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public string Message { get; set; }
        public string ErrorMessage { get; set; } // Добавляем для ошибок

        // Внутренний класс для валидации
        public class InputModel
        {
            [Required(ErrorMessage = "Title is required.")]
            public string Title { get; set; }
            public string Text { get; set; }
            public List<Image> Images { get; set; }
        }

        public void OnGet()
        {
            ThreadInput = new InputModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Создаём объект Tread из ThreadInput
            var thread = new Tread
            {
                Title = ThreadInput.Title,
                Text = ThreadInput.Text,
                Images = ThreadInput.Images ?? new List<Image>()
            };

            // Обрабатываем изображение
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

            // Сохраняем тред
            _context.Treads.Add(thread);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thread created successfully!";
            return RedirectToPage("./Threads");
        }
    }
}