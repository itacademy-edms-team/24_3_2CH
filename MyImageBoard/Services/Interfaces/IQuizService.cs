using ForumProject.Data.Models;

namespace ForumProject.Services.Interfaces
{
    public interface IQuizService
    {
        /// <summary>
        /// Создает новый опрос для треда
        /// </summary>
        Task<Quiz?> CreateQuizAsync(int threadId, string question, List<string> options, bool isMultiple);

        /// <summary>
        /// Проверяет, голосовал ли пользователь в опросе
        /// </summary>
        Task<bool> HasUserVotedAsync(int quizId, int fingerprintId);

        /// <summary>
        /// Добавляет голос пользователя в опрос
        /// </summary>
        Task AddVoteAsync(int[] optionIds, int fingerprintId);

        /// <summary>
        /// Получает статистику по опросу
        /// </summary>
        Task<Dictionary<int, int>> GetQuizStatisticsAsync(int quizId);

        /// <summary>
        /// Получает ID треда по ID опроса
        /// </summary>
        Task<int> GetThreadIdByQuizIdAsync(int quizId);

        /// <summary>
        /// Получает ID опроса по ID варианта ответа
        /// </summary>
        Task<int> GetQuizIdByOptionIdAsync(int optionId);
    }
} 