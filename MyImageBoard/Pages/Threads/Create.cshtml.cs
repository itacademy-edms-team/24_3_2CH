using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations; // ������ ���� using ��� ValidationAttribute
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ForumProject.Pages.Threads
{
    public class CreateModel : PageModel
    {
        private readonly IThreadService _threadService;
        private readonly IBoardService _boardService; // ����� ��� ��������� �������� �����
        private readonly IMediaFileService _mediaFileService;
        private readonly IQuizService _quizService;

        public CreateModel(
            IThreadService threadService, 
            IBoardService boardService,
            IMediaFileService mediaFileService,
            IQuizService quizService)
        {
            _threadService = threadService;
            _boardService = boardService;
            _mediaFileService = mediaFileService;
            _quizService = quizService;
        }

        [BindProperty]
        public SiteThread Thread { get; set; } = new SiteThread();

        [BindProperty]
        public IFormFileCollection? Files { get; set; }

        [BindProperty]
        public string Quizzes { get; set; } = "[]";

        public string BoardTitle { get; set; } = string.Empty; //    

        // ,    HTTP GET  
        //  boardId  URL
        public async Task<IActionResult> OnGetAsync(int boardId)
        {
            var board = await _boardService.GetBoardByIdAsync(boardId);
            if (board == null)
            {
                return NotFound(); //    ,  404
            }

            Thread.BoardId = boardId; //  BoardId   
            BoardTitle = board.Title; //     

            return Page();
        }

        // ,    HTTP POST  ( )
        public async Task<IActionResult> OnPostAsync()
        {
            //  BoardId   (,      /Threads/Create  ID)
            if (Thread.BoardId == 0)
            {
                ModelState.AddModelError(string.Empty, "Не удалось определить доску для создания треда.");
                return Page();
            }

            //   , ,  BoardId ����������
            var board = await _boardService.GetBoardByIdAsync(Thread.BoardId);
            if (board == null)
            {
                ModelState.AddModelError(string.Empty, "Указанная доска не существует.");
                return Page();
            }

            if (ModelState.ContainsKey("Thread.Board"))
            {
                ModelState.Remove("Thread.Board");
            }

            BoardTitle = board.Title; // ��������������� �������� �����, ���� ��������� �� ������

            // ��������� ��������� ������ (��������, Required, MaxLength �� SiteThread.cs)
            if (!ModelState.IsValid)
            {
                return Page(); // ���� ������ ���������, ���������� ����� ����� � ��������
            }

            // Валидация файлов, если они есть
            if (Files != null && Files.Any())
            {
                var (isValid, errorMessage) = await _mediaFileService.ValidateFilesAsync(Files);
                if (!isValid)
                {
                    ModelState.AddModelError(string.Empty, errorMessage ?? "Ошибка при валидации файлов");
                    return Page();
                }
            }

            // Создаем тред и сохраняем файлы
            var (createdThread, error) = await _threadService.CreateThreadAsync(Thread, Files);
            if (createdThread == null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Ошибка при создании треда");
                return Page();
            }

            // Создаем опросы
            try
            {
                var quizzesList = JsonSerializer.Deserialize<List<QuizCreateRequest>>(Quizzes);
                if (quizzesList != null && quizzesList.Any())
                {
                    foreach (var quiz in quizzesList)
                    {
                        var createdQuiz = await _quizService.CreateQuizAsync(
                            createdThread.Id,
                            quiz.Question,
                            quiz.Options,
                            quiz.IsMultiple
                        );

                        if (createdQuiz == null)
                        {
                            ModelState.AddModelError(string.Empty, "Не удалось создать один из опросов");
                            return Page();
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                ModelState.AddModelError(string.Empty, "Ошибка при обработке данных опросов: " + ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ошибка при создании опросов: " + ex.Message);
                return Page();
            }

            //     ,    
            return RedirectToPage("/Threads/Details", new { id = createdThread.Id });
        }

        public class QuizCreateRequest
        {
            public string Question { get; set; } = string.Empty;
            public List<string> Options { get; set; } = new();
            public bool IsMultiple { get; set; }
        }
    }
}