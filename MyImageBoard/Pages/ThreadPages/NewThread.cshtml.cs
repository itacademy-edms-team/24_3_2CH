using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

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
        public Tread Thread { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Инициализируем коллекцию Images, если она null
            Thread.Images = Thread.Images ?? new List<Image>();

            // Сохраняем тред (без изображений пока)
            _context.Treads.Add(Thread);
            await _context.SaveChangesAsync();

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

                // Создаем объект Image и добавляем его в коллекцию Thread.Images
                var image = new Image
                {
                    ImageUrl = "/images/" + uniqueFileName
                };
                Thread.Images.Add(image);

                // Сохраняем изменения (EF Core сам обновит таблицу Tread_Image)
                await _context.SaveChangesAsync();
            }

            Message = "Thread created successfully!";
            Thread = new Tread();

            return Page();
        }
    }
}