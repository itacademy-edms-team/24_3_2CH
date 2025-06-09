using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ForumProject.Services.Interfaces;

namespace ForumProject.Pages.Quiz
{
    public class VoteModel : PageModel
    {
        private readonly IQuizService _quizService;
        private readonly IUserFingerprintService _fingerprintService;

        public VoteModel(IQuizService quizService, IUserFingerprintService fingerprintService)
        {
            _quizService = quizService;
            _fingerprintService = fingerprintService;
        }

        public async Task<IActionResult> OnPostAsync([FromForm] int quizId, [FromForm] string optionIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Преобразуем строку с ID опций в массив
                var optionIdsArray = optionIds.Split(',').Select(int.Parse).ToArray();

                // Получаем fingerprint из куки
                var fingerprint = await _fingerprintService.GetOrCreateFingerprintAsync(HttpContext);
                
                // Добавляем голос
                await _quizService.AddVoteAsync(optionIdsArray, fingerprint.Id);

                // Получаем ID треда для редиректа
                var threadId = await _quizService.GetThreadIdByQuizIdAsync(quizId);

                // Возвращаемся на страницу треда
                return RedirectToPage("/Threads/Details", new { id = threadId });
            }
            catch (InvalidOperationException ex)
            {
                // Пользователь уже голосовал
                TempData["Error"] = ex.Message;
                var threadId = await _quizService.GetThreadIdByQuizIdAsync(quizId);
                return RedirectToPage("/Threads/Details", new { id = threadId });
            }
            catch (Exception ex)
            {
                // Другие ошибки
                TempData["Error"] = "Произошла ошибка при голосовании";
                var threadId = await _quizService.GetThreadIdByQuizIdAsync(quizId);
                return RedirectToPage("/Threads/Details", new { id = threadId });
            }
        }
    }
} 